using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;
using UnityEngine;

namespace Gameplay
{
    public class Inventory
    {
        public Action<Gameplay.ItemInstance> OnItemAdded;
        public Action<Gameplay.ItemInstance> OnItemChargeChanged;
        public Action<Gameplay.ItemInstance> OnItemRemoved;
        
        /// <summary>
        /// For stacks of items, Item has a Quantity that should be updated instead of trying to add collisions to the dictionary.
        /// </summary>
        private List<ItemInstance> Items = new List<ItemInstance>();
        
        public IReadOnlyList<ItemInstance> GetAllItems() => Items.ToList();

        private bool IsPlayerInventoy = false;

        public Inventory(bool isPlayerInventoy)
        {
            IsPlayerInventoy = isPlayerInventoy;
        }
        
        /// <summary>
        /// Returns if we have the item in the inventory. Even if it's empty.
        /// </summary>
        public bool HasItem(ItemInstance.Id itemId)
        {
            return Items.Any(item => item.Schema.Id ==itemId);
        }

        public ItemInstance GetFirstItem(ItemInstance.Id itemId)
        {
            return Items.First(item => item.Schema.Id == itemId);
        }

        /// <summary>
        /// Returns whether or not the itemId can be added to the inventory. Handles stacking logic.
        /// </summary>
        public bool AddItem(ItemInstance.Id itemId)
        {
            if (HasItem(itemId))
            {
                // TODO: Better logic here?
                // Consumable items should stack on their consumable count
                var item = GetFirstItem(itemId);
                if (item.Schema.IsConsumbale)
                {
                    item.AddCharge(1);
                    OnItemChargeChanged?.Invoke(item);
                    return true;
                }

                // Do not add more than one unique equipped
                if (item.Schema.IsUniqueEquipped)
                {
                    return false;
                }
            }
            
            foreach (var itemSchema in ServiceLocator.Instance.Schemas.ItemSchemas)
            {
                if (itemSchema.Id != itemId)
                {
                    continue;
                }

                var newItem = new ItemInstance(itemSchema);
                Items.Add(newItem);
                OnItemAdded?.Invoke(newItem);

                // TODO: REFACTOR candidate
                if (IsPlayerInventoy)
                {
                    newItem.ApplyPassiveEffects(ServiceLocator.Instance.Player);
                }
                
                return true;
            }

            return false;
        }
        
        public void RemoveItem(ItemInstance item)
        {
            Items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        public bool UseItem(ItemInstance itemInstance)
        {
            if (!itemInstance.CanBeUsed())
            {
                return false;
            }

            if (itemInstance.Schema.IsConsumbale)
            {
                itemInstance.RemoveCharge(1);
                OnItemChargeChanged?.Invoke(itemInstance);
                itemInstance.ApplyActiveEffects(ServiceLocator.Instance.Player);
                return true;
            }
            
            return false;
        }
    }
    
    public class ItemInstance
    {
        // TODO: Move to ItemSchema??
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
            
            DetectorRat,
            DetectorBat,
            DetectorBrick,
            
            SacrificialKris,
            
            BaitRat,
            BaitBat,
            BaitFaerie,
            Egg,
            PizzaSlice,
            
            Candle,
        }

        public ItemSchema Schema;
        public int MaxQuantity;
        
        private int CurrentQuantity;

        public ItemInstance (ItemSchema schema)
        {
            Schema = schema;
            CurrentQuantity = schema.InitialCharges;
            MaxQuantity = schema.InitialCharges;
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

        private void ApplyEffects(Player player, Effect[] effects)
        {
            // Just in case
            if (effects == null)
            {
                return;
            }
            
            foreach (Effect effect in effects)
            {
                switch (effect.Type)
                {
                    case EffectType.BonusHP:
                        player.AddBonusStartHp(effect.Amount);
                        break;
                    
                    case EffectType.BonusXP:
                        player.AddBonusStartXp(effect.Amount);
                        break;
                    
                    case EffectType.AutoReveal:
                        for (int i = 0; i < effect.Amount; i++)
                        {
                            player.AddMonsterToAutoRevealedList(effect.Id);
                        }
                        break;
                    
                    case EffectType.ModDamageTaken:
                        if (!string.IsNullOrEmpty(effect.Id))
                        {
                            player.AddModDamageTaken(effect.Id, effect.Amount);
                        }
                        
                        foreach (var effectTag in effect.Tags)
                        {
                            player.AddModDamageTakenByTag(effectTag, effect.Amount);
                        }
                        
                        break;
                    
                    case EffectType.ModXp:
                        if (!string.IsNullOrEmpty(effect.Id))
                        {
                            player.AddModXp(effect.Id, effect.Amount);
                        }
                        
                        foreach (var effectTag in effect.Tags)
                        {
                            player.AddModXpByTag(effectTag, effect.Amount);
                        }
                        break;
                    
                    case EffectType.BonusSpawn:
                        if (!string.IsNullOrEmpty(effect.Id))
                        {
                            player.AddSpawnCount(effect.Id, effect.Amount);
                        }
                        // TODO: Support by Tag?
                        break;
                    
                    case EffectType.UpgradeTileObject:
                        if (!string.IsNullOrEmpty(effect.Id))
                        {
                            player.AddOrIncrementTileLevel(effect.Id);
                        }
                        // TODO: Support by Tag?
                        break;
                }
            }
        }
        public void ApplyPassiveEffects(Player player)
        {
            ApplyEffects(player, Schema.PassiveEffects);
        }
        
        public void ApplyActiveEffects(Player player)
        {
            ApplyEffects(player, Schema.ActiveEffects);
        }
    }
}