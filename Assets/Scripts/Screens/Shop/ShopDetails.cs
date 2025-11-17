using Gameplay;
using Schemas;
using Screens.Inventory;
using System.Drawing;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Screens.Shop
{
    // TODO: REFACTOR ShopDetails vs InventoryDetails.... damn it
    public class ShopDetails : InventoryDetails
    {
        protected override void HandleButtonClicked()
        {
            var player = ServiceLocator.Instance.Player;

            // Check for price
            if (player.ShopXp < GetPrice())
            {
                return;
            }

            // Play purchase sfx
            ServiceLocator.Instance.AudioManager.PlaySfx("Purchase");

            // Deduct cost
            player.ShopXp -= GetPrice();

            // Add it to the player's inventory
            player.Inventory.AddItem(ItemInstance.Schema.ItemId);

            // Once you but the item, you can't buy it again. We'll remove it from the shop
            ShopScreen shopScreen = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
            shopScreen.RemoveItem(ItemInstance);
            shopScreen.ClearFocusedItem();
            shopScreen.RefreshRerollText();

            // And you can't press the buy button until another selection is made
            Button.interactable = false;
        }

        private int GetPrice()
        {
            return ItemInstance.IsOnSale ? ItemInstance.Schema.Price / 2 : ItemInstance.Schema.Price;
        }

        public override void SetItem(ItemInstance itemInstance)
        {
            base.SetItem(itemInstance);

            var buttonText = Button.GetComponentInChildren<TMP_Text>();
            buttonText.SetText($"Buy ${GetPrice()}");
            Button.interactable = ServiceLocator.Instance.Player.ShopXp >= GetPrice();

            buttonText.color = itemInstance.IsOnSale ? Color.green : Color.white;
        }
    }
}