using System.Collections.Generic;
using Schemas;

namespace Gameplay
{
    public class Inventory
    {
        /// <summary>
        /// For stacks of items, Item has a Quantity that should be updated instead of trying to add collisions to the dictionary.
        /// </summary>
        Dictionary<Item.Id, Item> Items = new Dictionary<Item.Id, Item>();

        /// <summary>
        /// Returns if we have the item in the inventory. Even if it's empty.
        /// </summary>
        public bool HasItem(Item.Id itemId)
        {
            return Items.ContainsKey(itemId);
        }

        /// <summary>
        /// Returns whether or not the itemId can be added to the inventory. Handles stacking logic.
        /// </summary>
        public bool AddItem(Item.Id itemId)
        {
            if (Items.TryGetValue(itemId, out Item item))
            {
                if (item.IsConsumable)
                {
                    // TODO: more logic here
                    return true;
                }

                return false;
            }
            
            foreach (var itemSchema in ServiceLocator.Instance.Schemas.ItemSchemas)
            {
                if (itemSchema.Id != itemId)
                {
                    continue;
                }

                Items.Add(itemId, new Item(itemSchema));
                return true;
            }

            return false;
        }
    }

    public class Item
    {
        public enum Id
        {
            // Used for Empty
            None,
            
            // Class starting items w
            Sword,
            Bow,
            MagicCarpet,
            Flute,
            TarotDeck,
            HolyLight,
            Alembic,
            
            // More items...
        }

        public Id ItemId;
        public bool IsConsumable;
        public int MaxQuantity;
        
        private int CurrentQuantity;

        public Item (ItemSchema schema)
        {
            ItemId = schema.Id;
            
            // TODO: More logic here
        }
    }
}