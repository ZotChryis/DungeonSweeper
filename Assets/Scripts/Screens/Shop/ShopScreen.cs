using Gameplay;

namespace Screens.Shop
{
    // TODO: Probably should have a UI vs Controller setup, but I am getting tired tonight lol
    // TODO: Refactor ShopScreen vs InventoryScreen - probably a better way to organize this
    public class ShopScreen : InventoryScreen
    {
        protected override void SetupInventory()
        {
            Inventory = new Gameplay.Inventory();
        }
        
        /// <summary>
        /// Adds items to the shop given the current level.
        /// </summary>
        /// TODO: Make Level matter
        public void Roll(int level)
        {
            // Get all the items in the game
            var allItems = ServiceLocator.Instance.Schemas.ItemSchemas;
            
            // Remove any items that they already own (and aren't consumables)
            allItems.RemoveAll(schema =>
                !schema.IsConsumbale && ServiceLocator.Instance.Player.Inventory.HasItem(schema.Id)
            );
            
            // Roll by rarity to see if they are included
            foreach (var itemSchema in allItems)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) <= itemSchema.GetSuccessRate())
                {
                    Inventory.AddItem(itemSchema.Id);
                }
            }
        }

        public void RemoveItem(Item.Id itemId)
        {
            Inventory.RemoveItem(itemId);
        }
    }
}