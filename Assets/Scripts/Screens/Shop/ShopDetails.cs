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
            // Check for price
            if (ServiceLocator.Instance.Player.ShopXp < Item.Schema.Price)
            {
                return;
            }

            // Add it to the player's inventory
            ServiceLocator.Instance.Player.Inventory.AddItem(Item.Schema.Id);
            
            // Once you but the item, you can't buy it again. We'll remove it from the shop
            ShopScreen shopScreen = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Shop] as ShopScreen;
            shopScreen.RemoveItem(Item.Schema.Id);
            shopScreen.ClearFocusedItem();
            
            // And you can't press the buy button until another selection is made
            Button.interactable = false;
        }

        public override void SetItem(Item item)
        {
            base.SetItem(item);

            Button.GetComponentInChildren<TMP_Text>().SetText($"Buy ${item.Schema.Price}");
            Button.interactable = ServiceLocator.Instance.Player.ShopXp >= Item.Schema.Price;
        }
    }
}