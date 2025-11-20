using System;
using System.Collections.Generic;
using Gameplay;
using Schemas;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Shop
{
    // TODO: Probably should have a UI vs Controller setup, but I am getting tired tonight lol
    // TODO: Refactor ShopScreen vs InventoryScreen - probably a better way to organize this
    public class ShopScreen : InventoryScreen
    {
        [SerializeField] private Button Reroll;
        [SerializeField] private Button Continue;
        [SerializeField] private Sprite ItemsDisabledIcon;

        private int onShowMoney;

        protected override void Awake()
        {
            base.Awake();

            Continue.onClick.AddListener(OnContinueClicked);
            Reroll.onClick.AddListener(OnRerollClicked);

            ServiceLocator.Instance.Player.OnShopXpChanged += OnShopXpChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ServiceLocator.Instance.Player.OnShopXpChanged -= OnShopXpChanged;
        }

        protected override void OnShow()
        {
            base.OnShow();

            RefreshRerollText();
            Roll(ServiceLocator.Instance.LevelManager.CurrentLevel, true);
            ServiceLocator.Instance.TutorialManager.TryShowTutorial(TutorialManager.TutorialId.Shop);
            onShowMoney = ServiceLocator.Instance.Player.ShopXp;
        }

        private void OnShopXpChanged()
        {
            Reroll.interactable = ServiceLocator.Instance.Player.ShopXp >= 2;
        }

        private void OnContinueClicked()
        {
            if (onShowMoney <= ServiceLocator.Instance.Player.ShopXp && ServiceLocator.Instance.Player.Class != Class.Id.Ascetic)
            {
                ServiceLocator.Instance.OverlayScreenManager.RequestConfirmationScreen(() =>
                {
                    ContinueFromShopToNextLevel();
                },
                    "Are you sure?",
                    "Are you sure you want to continue without buying anything?"
                );
            }
            else
            {
                ContinueFromShopToNextLevel();
            }
        }

        private void ContinueFromShopToNextLevel()
        {
            // IN THE DEMO YOU CAN ONLY CONTINUE FROM LEVEL 0.
            // Play through level 1. Then the game stops.
            if (ServiceLocator.Instance.LevelManager.CurrentLevel == 0)
            {
                // TODO: This logic needs to go somewhere else...
                // Reset the player, close the shop and update the level
                ServiceLocator.Instance.Player.ResetPlayer();
                ServiceLocator.Instance.LevelManager.NextLevel();
                ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();

                ServiceLocator.Instance.SaveSystem.SaveGame();
            }
            else
            {
                ServiceLocator.Instance.OverlayScreenManager.RequestShowScreen(OverlayScreenManager.ScreenType.SteamDemoUpsell);
            }
        }

        private void OnRerollClicked()
        {
            if (ServiceLocator.Instance.Player.ShopXp < 2)
            {
                return;
            }

            // Reroll card reduces cost from 2 -> 1
            bool hasRerollCard = ServiceLocator.Instance.Player.Inventory.HasItem(ItemSchema.Id.RerollCreditCard);
            ServiceLocator.Instance.Player.ShopXp -= hasRerollCard ? 1 : 2;

            Roll(ServiceLocator.Instance.LevelManager.CurrentLevel, false);

            ServiceLocator.Instance.AchievementSystem.CompleteAchievementById(AchievementSchema.Id.ShopReroll);
        }

        public void RefreshRerollText()
        {
            bool hasRerollCard = ServiceLocator.Instance.Player.Inventory.HasItem(ItemSchema.Id.RerollCreditCard);
            Reroll.GetComponentInChildren<TMP_Text>().SetText(hasRerollCard ? "$1 Reroll" : "$2 Reroll");
        }

        protected override void SetupInventory()
        {
            Inventory = new Gameplay.Inventory(false);
        }

        /// <summary>
        /// Adds items to the shop given the current level.
        /// </summary>
        /// TODO: Make Level matter more??
        public void Roll(int level, bool limitShopInventoryByPlayerXp)
        {
            // Clear current shop
            foreach (var itemInstance in Inventory.GetAllItems())
            {
                Inventory.RemoveItem(itemInstance);
            }

            // skip this step completely if items are not allowed.
            if (ServiceLocator.Instance.Player.Class == Class.Id.Ascetic)
            {
                ToastManager.Instance.RequestToast(ItemsDisabledIcon, "Items Disabled!", "Items disabled from Ascetic!");
                return;
            }

            // Get all the items in the game
            List<ItemSchema> allItems = new List<ItemSchema>();
            allItems.AddRange(ServiceLocator.Instance.Schemas.ItemSchemas);

            // Remove any items that they already have max copies of
            allItems.RemoveAll(schema =>
                schema.Max != -1 && ServiceLocator.Instance.Player.Inventory.GetItemCount(schema.ItemId) >= schema.Max
            );

            // Remove any item that is locked
            var lockedItemIds = ServiceLocator.Instance.AchievementSystem.GetLockedItems();
            foreach (var lockedItemId in lockedItemIds)
            {
                allItems.RemoveAll(schema => schema.ItemId == lockedItemId);
            }

            allItems.Shuffle();
            allItems.Sort((i1, i2) => i1.Rarity.CompareTo(i2.Rarity));

            var player = ServiceLocator.Instance.Player;

            // Check for price
            var maxItemCostAllowed = limitShopInventoryByPlayerXp ? player.ShopXp : 9999;

            // Roll by rarity to see if they are included
            float addedChanceFromLevel = level * 0.1f;
            var numberOfItemsAddedPerRarity = new Dictionary<Rarity, int>(5);
            foreach (var itemSchema in allItems)
            {
                if (itemSchema.Price > maxItemCostAllowed)
                {
                    //Debug.Log("Item: " + itemSchema.ItemId + " is too expensive at price " + itemSchema.Price + ", skipping spawn of item.");
                    continue;
                }
                if (itemSchema.ShopInventory <= 0)
                {
                    //Debug.Log("Skipping item " + itemSchema.ItemId + " because shop inventory is " + itemSchema.ShopInventory);
                    continue;
                }
                numberOfItemsAddedPerRarity.TryGetValue(itemSchema.Rarity, out int numberOfItemsOfRarityAlreadyAdded);
                if (numberOfItemsOfRarityAlreadyAdded >= itemSchema.Rarity.GetMaxItemQuantityForLevel(level))
                {
                    //Debug.Log("Already spawned " + numberOfItemsOfRarityAlreadyAdded + " for rarity " + itemSchema.Rarity + " on level " + level + ", skipping item spawn.");
                    continue;
                }
                if (UnityEngine.Random.Range(0.0f, 1.0f) <= itemSchema.GetShopAppearanceRate() + addedChanceFromLevel)
                {
                    //Debug.Log("Success on " + (itemSchema.GetShopAppearanceRate() + addedChanceFromLevel) + " chance. Spawning item : " + itemSchema.ItemId);
                    // TODO: Do inventory better instead of adding multiple entries??
                    for (int i = 0; i < itemSchema.ShopInventory; i++)
                    {
                        Inventory.AddItem(itemSchema.ItemId);
                    }
                    numberOfItemsAddedPerRarity[itemSchema.Rarity] = numberOfItemsOfRarityAlreadyAdded + 1;
                }
            }

            // Offer sales if the user has the credit card
            if (ServiceLocator.Instance.Player.Inventory.HasItem(ItemSchema.Id.CreditCard))
            {
                // Two items will be on sale
                var items = new List<ItemInstance>();
                items.AddRange(Inventory.GetAllItems());
                items.Shuffle();
                for (int i = 0; i < items.Count; i++)
                {
                    var saleItem = items[i];
                    saleItem.IsOnSale = i < 3;
                }
            }
        }

        public void RemoveItem(ItemInstance item)
        {
            Inventory.RemoveItem(item);
        }

        public void CheatRollAll()
        {
            // Clear current shop
            foreach (var itemInstance in Inventory.GetAllItems())
            {
                Inventory.RemoveItem(itemInstance);
            }

            // Get all the items in the game
            List<ItemSchema> allItems = new List<ItemSchema>();
            allItems.AddRange(ServiceLocator.Instance.Schemas.ItemSchemas);
            allItems.Sort((i1, i2) => i1.Rarity.CompareTo(i2.Rarity));

            // Roll by rarity to see if they are included
            foreach (var itemSchema in allItems)
            {
                for (int i = 0; i < itemSchema.ShopInventory; i++)
                {
                    Inventory.AddItem(itemSchema.ItemId);
                }
            }
        }
    }
}