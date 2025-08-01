﻿using System.Collections.Generic;
using Gameplay;
using Schemas;
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

        protected override void Awake()
        {
            base.Awake();
            Continue.onClick.AddListener(OnContinueClicked);
            Reroll.onClick.AddListener(OnRerollClicked);

            ServiceLocator.Instance.Player.OnShopXpChanged += OnShopXpChanged;
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            Roll(ServiceLocator.Instance.LevelManager.CurrentLevel);
            ServiceLocator.Instance.TutorialManager.TryShowTutorial(TutorialManager.TutorialId.Shop);
        }

        private void OnShopXpChanged()
        {
            Reroll.interactable = ServiceLocator.Instance.Player.ShopXp >= 2;
        }
        
        private void OnContinueClicked()
        {
            // TODO: This logic needs to go somewhere else...
            // Reset the player, close the shop and update the level
            ServiceLocator.Instance.LevelManager.NextLevel();
            ServiceLocator.Instance.Player.ResetPlayer();
            ServiceLocator.Instance.OverlayScreenManager.HideAllScreens();
        }
        
        private void OnRerollClicked()
        {
            if (ServiceLocator.Instance.Player.ShopXp < 2)
            {
                return;
            }

            ServiceLocator.Instance.Player.ShopXp -= 2;
            Roll(ServiceLocator.Instance.LevelManager.CurrentLevel);
        }

        protected override void SetupInventory()
        {
            Inventory = new Gameplay.Inventory(false);
        }
        
        /// <summary>
        /// Adds items to the shop given the current level.
        /// </summary>
        /// TODO: Make Level matter more??
        public void Roll(int level)
        {
            // Clear current shop
            foreach (var itemInstance in Inventory.GetAllItems())
            {
                Inventory.RemoveItem(itemInstance);
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
            
            allItems.Sort((i1, i2) => i1.Rarity.CompareTo(i2.Rarity));
            
            // Roll by rarity to see if they are included
            float addedChanceFromLevel = level * 0.1f;
            foreach (var itemSchema in allItems)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) <= itemSchema.GetShopAppearanceRate() + addedChanceFromLevel)
                {
                    // TODO: Do inventory better instead of adding multiple entries??
                    for (int i = 0; i < itemSchema.ShopInventory; i++)
                    {
                        Inventory.AddItem(itemSchema.ItemId);
                    }
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