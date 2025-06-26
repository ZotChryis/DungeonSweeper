using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;

namespace Gameplay
{
    public class Inventory
    {
        public Action<Gameplay.Item> OnItemAdded;
        public Action<Gameplay.Item> OnItemChargeChanged;
        public Action<Gameplay.Item> OnItemRemoved;
        
        /// <summary>
        /// For stacks of items, Item has a Quantity that should be updated instead of trying to add collisions to the dictionary.
        /// </summary>
        Dictionary<Item.Id, Item> Items = new Dictionary<Item.Id, Item>();

        public List<Item> GetAllItems() => Items.Values.ToList();
        
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
                if (item.Schema.IsConsumbale)
                {
                    item.AddCharge(1);
                    OnItemChargeChanged?.Invoke(item);
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

                var newItem = new Item(itemSchema);
                Items.Add(itemId, newItem);
                OnItemAdded?.Invoke(newItem);
                return true;
            }

            return false;
        }
        
        public void RemoveItem(Item.Id itemId)
        {
            Items.Remove(itemId);
        }

        public bool UseItem(Item item)
        {
            if (!item.CanBeUsed())
            {
                return false;
            }

            if (item.Schema.IsConsumbale)
            {
                item.RemoveCharge(1);
                OnItemChargeChanged?.Invoke(item);
            }
            
            return true;
        }
    }
    
    public class Item
    {
        // !!WARNING!! DO NOT REORDER
        public enum Id
        {
            // Used for Empty
            None,
            
            // Class starting items
            Sword,
            Bow,
            MagicCarpet,
            Flute,
            TarotDeck,
            HolyLight,
            Alembic,
            
            // More items...
            RatRepellent,
            MeatGrinder,
            Campfire,
            Abacus,
        }

        public ItemSchema Schema;
        public Id ItemId;
        public int MaxQuantity;
        
        private int CurrentQuantity;

        public Item (ItemSchema schema)
        {
            Schema = schema;
            CurrentQuantity = schema.InitialCharges;
            MaxQuantity = schema.InitialCharges;

            // TODO: More logic here
        }

        public void AddCharge(int amount)
        {
            CurrentQuantity += amount;
            MaxQuantity += amount;
        }

        public void RemoveCharge(int amount)
        {
            CurrentQuantity -= amount;
        }

        public bool CanBeUsed()
        {
            return Schema.IsConsumbale && CurrentQuantity >= 0;
        }
    }
}