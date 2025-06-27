using Gameplay;
using Screens.Inventory;
using TMPro;

namespace Screens.Shop
{
    // TODO: REFACTOR ShopDetails vs InventoryDetails.... damn it
    public class ShopDetails : InventoryDetails
    {
        protected override void HandleButtonClicked()
        {
            var player = ServiceLocator.Instance.Player;
                
            // Check for price
            if (player.ShopXp < ItemInstance.Schema.Price)
            {
                return;
            }

            // Deduct cost
            player.ShopXp -= ItemInstance.Schema.Price;
            
            // Add it to the player's inventory
            player.Inventory.AddItem(ItemInstance.Schema.ItemId);
            
            // Once you but the item, you can't buy it again. We'll remove it from the shop
            ShopScreen shopScreen = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
            shopScreen.RemoveItem(ItemInstance);
            shopScreen.ClearFocusedItem();
            
            // And you can't press the buy button until another selection is made
            Button.interactable = false;
        }

        public override void SetItem(ItemInstance itemInstance)
        {
            base.SetItem(itemInstance);

            Button.GetComponentInChildren<TMP_Text>().SetText($"Buy ${itemInstance.Schema.Price}");
            Button.interactable = ServiceLocator.Instance.Player.ShopXp >= ItemInstance.Schema.Price;
        }
    }
}