using System;
using Gameplay;
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

        protected void OnEnable()
        {
            Roll(ServiceLocator.Instance.LevelManager.CurrentLevel);
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
        /// TODO: Make Level matter
        public void Roll(int level)
        {
            // Clear current shop
            foreach (var itemInstance in Inventory.GetAllItems())
            {
                Inventory.RemoveItem(itemInstance);
            }
            
            // Get all the items in the game
            var allItems = ServiceLocator.Instance.Schemas.ItemSchemas;
            
            // Remove any items that they already own (and aren't consumables)
            allItems.RemoveAll(schema =>
                !schema.IsConsumbale && ServiceLocator.Instance.Player.Inventory.HasItem(schema.ItemId)
            );
            
            // Roll by rarity to see if they are included
            foreach (var itemSchema in allItems)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) <= itemSchema.GetShopAppearanceRate())
                {
                    for (int i = 0; i < itemSchema.ShopInventory; i++)
                    {
                        Inventory.AddItem(itemSchema.ItemId);
                    }
                }
            }
            
            // TODO: choose 2 items to  be on sale each shop
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
            var allItems = ServiceLocator.Instance.Schemas.ItemSchemas;
            
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