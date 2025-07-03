using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;
using UnityEngine;
using UnityEngine.EventSystems;

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

            if (IsPlayerInventoy)
            {
                ServiceLocator.Instance.Player.OnLevelChanged += OnPlayerLevelChanged;
                ServiceLocator.Instance.Player.OnConquer += OnConquer;
                ServiceLocator.Instance.LevelManager.OnLevelChanged += OnDungeonLevelChanged;
            }
        }

        private void OnConquer(TileSchema tileObject)
        {
            foreach (var itemInstance in Items)
            {
                if (!itemInstance.IsValidConquer(tileObject))
                {
                    continue;
                }
                
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Conquer);
            }
        }

        private void OnPlayerLevelChanged(int newLevel)
        {
            foreach (var itemInstance in Items)
            {
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.PlayerLevel);
            }
        }

        private void OnDungeonLevelChanged(int newLevel)
        {
            foreach (var itemInstance in Items)
            {
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.DungeonLevel);
            }
        }
        
        /// <summary>
        /// Returns if we have the item in the inventory. Even if it's empty.
        /// </summary>
        public bool HasItem(ItemSchema.Id itemId)
        {
            return Items.Any(item => item.Schema.ItemId ==itemId);
        }

        public ItemInstance GetFirstItem(ItemSchema.Id itemId)
        {
            return Items.First(item => item.Schema.ItemId == itemId);
        }

        /// <summary>
        /// Returns whether or not the itemId can be added to the inventory. Handles stacking logic.
        /// </summary>
        public bool AddItem(ItemSchema.Id itemId)
        {
            if (HasItem(itemId))
            {
                // Do not add more than one unique equipped
                var item = GetFirstItem(itemId);
                if (item.Schema.IsUniqueEquipped)
                {
                    return false;
                }
            }
            
            foreach (var itemSchema in ServiceLocator.Instance.Schemas.ItemSchemas)
            {
                if (itemSchema.ItemId != itemId)
                {
                    continue;
                }

                var newItem = new ItemInstance(itemSchema);
                Items.Add(newItem);
                OnItemAdded?.Invoke(newItem);

                // TODO: REFACTOR candidate
                if (IsPlayerInventoy)
                {
                    newItem.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Purchase);
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

            itemInstance.RemoveCharge(1);
            OnItemChargeChanged?.Invoke(itemInstance);
            itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Used);
            return true;
        }

        public void ReplenishItems(ItemSchema.Id[] itemIds)
        {
            if (itemIds == null)
            {
                return;
            }
            
            foreach (var item in Items)
            {
                if (!item.Schema.IsConsumbale)
                {
                    continue;
                }

                if (!itemIds.Contains(item.Schema.ItemId))
                {
                    continue;
                }

                int oldCharge = item.CurrentQuantity;
                item.ReplenishAllCharges();
                int newCharge = item.CurrentQuantity;

                if (oldCharge != newCharge)
                {
                    OnItemChargeChanged?.Invoke(item);   
                }
            }
        }
    }
    
    public class ItemInstance
    {
        public ItemSchema Schema;
        public int MaxQuantity;
        public int CurrentQuantity;

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
            return Schema.IsConsumbale && CurrentQuantity > 0;
        }

        // TODO: O(N^2) to check validity -> application, but im too lazy to fix the model
        // TODO: this conflates the Effect Data with the Trigger Conditions -- we need to separate them out
        public bool IsValidConquer(TileSchema tileSchema)
        {
            // Just in case
            if (!Schema.Effects.TryGetValue(EffectTrigger.Conquer, out Effect[] effects))
            {
                return false;
            }

            foreach (var effect in effects)
            {
                if (effect.Id != TileSchema.Id.None && effect.Id != tileSchema.TileId)
                {
                    continue;
                }

                if (effect.Id == TileSchema.Id.Global)
                {
                    return true;
                }
                
                if (tileSchema.Tags.Union(effect.Tags).ToList().Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void ApplyEffects(Player player, EffectTrigger trigger)
        {
            // Just in case
            if (!Schema.Effects.TryGetValue(trigger, out Effect[] effects))
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
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.AddModDamageTaken(effect.Id, effect.Amount);
                        }
                        
                        foreach (var effectTag in effect.Tags)
                        {
                            player.AddModDamageTakenByTag(effectTag, effect.Amount);
                        }
                        
                        break;
                    
                    case EffectType.ModXp:
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.AddModXp(effect.Id, effect.Amount);
                        }
                        
                        foreach (var effectTag in effect.Tags)
                        {
                            player.AddModXpByTag(effectTag, effect.Amount);
                        }
                        break;
                    
                    case EffectType.BonusSpawn:
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.AddSpawnCount(effect.Id, effect.Amount);
                        }
                        // TODO: Support by Tag?
                        break;
                    
                    case EffectType.UpgradeTileObject:
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.AddOrIncrementTileLevel(effect.Id);
                        }
                        // TODO: Support by Tag?
                        break;
                    
                    case EffectType.Damage:
                        // TODO: should the item be the source?
                        player.Damage(null, effect.Amount);
                        break;
                    
                    case EffectType.Heal:
                        // TODO: should the item be the source?
                        player.Heal(null, effect.Amount); //, false);
                        break;
                    
                    case EffectType.ChangeMoney:
                        player.ShopXp += effect.Amount;
                        break;
                    
                    case EffectType.RevealRandomLocation:
                        for (int i = 0; i < effect.Amount; i++)
                        {
                            ServiceLocator.Instance.Grid.RevealRandomUnoccupiedTile();
                        }
                        break;
                }
            }
        }

        public void ReplenishAllCharges()
        {
            CurrentQuantity = MaxQuantity;
        }
    }
}