using System;
using System.Collections.Generic;
using System.Linq;
using Schemas;

namespace Gameplay
{
    public class Inventory
    {
        public Action<ItemInstance> OnItemAdded;
        public Action<ItemInstance> OnItemChargeChanged;
        public Action<ItemInstance> OnItemRemoved;
        public Action<(ItemInstance, int)> OnItemStackChanged;
        
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
                ServiceLocator.Instance.Player.OnHeal += OnHeal;
                ServiceLocator.Instance.LevelManager.OnLevelChanged += OnDungeonLevelChanged;
            }
        }

        private void OnHeal(TileSchema tileObject)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var itemInstance  = Items[i];
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Heal);
            }
        }

        private void OnConquer(TileSchema tileObject)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var itemInstance  = Items[i];
                if (!itemInstance.IsValidConquer(tileObject))
                {
                    continue;
                }
                
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Conquer);
            }
        }

        private void OnPlayerLevelChanged(int newLevel)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var itemInstance  = Items[i];
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.PlayerLevel);
            }
        }

        private void OnDungeonLevelChanged(int newLevel)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var itemInstance = Items[i];
                itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.DungeonLevel);
                
                // Reset granted items since they can no longer be revoked
                itemInstance.KeepGrantedItems();
            }
        }
        
        /// <summary>
        /// Returns if we have the item in the inventory. Even if it's empty.
        /// </summary>
        public bool HasItem(ItemSchema.Id itemId)
        {
            return Items.Any(item => item.Schema.ItemId == itemId);
        }

        /// <summary>
        /// Returns the amount of copies of the item given.
        /// </summary>
        public int GetItemCount(ItemSchema.Id itemId)
        {
            int amount = 0;
            var matchingItems = Items.FindAll(item => item.Schema.ItemId == itemId);
            foreach (var matchingItem in matchingItems)
            {
                amount += matchingItem.StackCount;
            }

            return amount;
        }

        public ItemInstance GetFirstItem(ItemSchema.Id itemId)
        {
            return Items.First(item => item.Schema.ItemId == itemId);
        }

        /// <summary>
        /// Returns whether or not the itemId can be added to the inventory. Handles stacking logic.
        /// </summary>
        public ItemInstance AddItem(ItemSchema.Id itemId, bool forceAllowDuplicates = false)
        {
            if (!FBPP.GetBool(PlayerOptions.CanPickUpItems, true))
            {
                return null;
            }

            var item = ServiceLocator.Instance.Schemas.ItemSchemas.Find(item => item.ItemId == itemId);
            if (item == null)
            {
                return null;
            }
            
            if (!forceAllowDuplicates && item.Max != -1 && GetItemCount(itemId) >= item.Max)
            {
                return null;
            }

            if (item.CanStack && HasItem(itemId))
            {
                var itemInstance = GetFirstItem(itemId);
                itemInstance.StackCount += 1;
                
                // TODO: REFACTOR candidate
                if (IsPlayerInventoy)
                {
                    itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Purchase);
                }
                
                OnItemStackChanged?.Invoke((itemInstance, 1));
                return itemInstance;
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
                
                return newItem;
            }

            return null;
        }
        
        public void RemoveItem(ItemInstance item)
        {
            if (item.Schema.CanStack)
            {
                if (item.StackCount > 1)
                {
                    item.StackCount--;
                    
                    // TODO: REFACTOR candidate
                    if (IsPlayerInventoy)
                    {
                        item.UndoEffect(ServiceLocator.Instance.Player, EffectTrigger.Purchase);
                    }
                    
                    OnItemStackChanged?.Invoke((item, -1));
                    return;
                }
            }
            
            Items.Remove(item);
            
            // TODO: REFACTOR candidate
            if (IsPlayerInventoy)
            {
                item.UndoEffect(ServiceLocator.Instance.Player, EffectTrigger.Purchase);
            }
            
            OnItemRemoved?.Invoke(item);
        }

        public bool UseItem(ItemInstance itemInstance)
        {
            if (!itemInstance.CanBeUsed())
            {
                return false;
            }
            
            itemInstance.ApplyEffects(ServiceLocator.Instance.Player, EffectTrigger.Used);
            itemInstance.RemoveCharge(1);
            OnItemChargeChanged?.Invoke(itemInstance);
            
            // Play used sfx if available
            if (!string.IsNullOrEmpty(itemInstance.Schema.UseSfx))
            {
                ServiceLocator.Instance.AudioManager.PlaySfx(itemInstance.Schema.UseSfx);
            }
            
            return true;
        }

        public void ReplenishItems()
        {
            foreach (var item in Items)
            {
                if (!item.Schema.IsConsumbale)
                {
                    continue;
                }
                
                int oldCharge = item.CurrentCharges;
                item.ReplenishAllCharges();
                int newCharge = item.CurrentCharges;

                if (oldCharge != newCharge)
                {
                    OnItemChargeChanged?.Invoke(item);   
                }
            }
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

                int oldCharge = item.CurrentCharges;
                item.ReplenishAllCharges();
                int newCharge = item.CurrentCharges;

                if (oldCharge != newCharge)
                {
                    OnItemChargeChanged?.Invoke(item);   
                }
            }
        }

        public void RevertForRetry()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var itemInstance = Items[i];
                // TODO: EWWW this is gross. need to fix this somehow
                //  The issue is that some items like Alembic want to keep their granted items
                //  but others like Pickaxe should not. Prob should be a setting somewhere.
                //  The entire game was not build right for "retry" lol. Probably better if we serialize the game state
                //  and reload it instead of doing this shit but im too lazy. Maybe George wants to try and tackle that 
                //  nightmare?? lol
                if (itemInstance.Schema.ItemId == ItemSchema.Id.Pickaxe || 
                    itemInstance.Schema.ItemId == ItemSchema.Id.BusinessCard || 
                    itemInstance.Schema.ItemId == ItemSchema.Id.Dice || 
                    itemInstance.Schema.ItemId == ItemSchema.Id.BloodDonorCard
                ) {
                    itemInstance.RemoveGrantedItems();
                }
            }
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
    
    public class ItemInstance
    {
        public ItemSchema Schema;
        public int MaxCharges;
        private int _CurrentCharges;
        private int _StackCount;

        public Action<int> CurrentChargesChanged;
        public Action<int> StackCountChanged;
        public Action<bool> IsOnSaleChanged;

        public int CurrentCharges
        {
            set
            {
                _CurrentCharges = value;
                CurrentChargesChanged?.Invoke(_CurrentCharges);
            }
            get
            {
                return _CurrentCharges;
            }
        }
        
        public bool IsOnSale
        {
            set
            {
                _IsOnSale = value;
                IsOnSaleChanged?.Invoke(_IsOnSale);
            }
            get
            {
                return _IsOnSale;
            }
        }

        public int StackCount
        {
            set
            {
                _StackCount = value;
                StackCountChanged?.Invoke(_StackCount);
            }
            get
            {
                return _StackCount;
            }
        }

        private bool _IsOnSale = false;
        private List<ItemInstance> GrantedItems = new List<ItemInstance>();

        public ItemInstance (ItemSchema schema)
        {
            Schema = schema;
            CurrentCharges = schema.InitialCharges;
            MaxCharges = schema.InitialCharges;
            StackCount = 1;
        }

        public void AddCharge(int amount)
        {
            CurrentCharges += amount;
            MaxCharges += amount;
        }

        public void RemoveCharge(int amount)
        {
            CurrentCharges -= amount;
        }

        public bool CanBeUsed()
        {
            return Schema.IsConsumbale && CurrentCharges > 0;
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
                if (effect.Id != TileSchema.Id.None && effect.Id == tileSchema.TileId)
                {
                    return true;
                }

                // TODO: This can be replaced with Enemy tag now
                if (effect.Id == TileSchema.Id.Global)
                {
                    return true;
                }
                
                if (tileSchema.Tags.Intersect(effect.Tags).ToList().Count > 0)
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
                            if (effect.Id != TileSchema.Id.None)
                            {
                                player.AddMonsterToAutoRevealedList(effect.Id);
                            }

                            if (effect.Tags.Count > 0)
                            {
                                foreach (var effectTag in effect.Tags)
                                {
                                    player.AddMonsterToAutoRevealedByTagList(effectTag);
                                }
                            }
                        }
                        break;
                    
                    case EffectType.ModDamageTaken:
                        if (effect.Id == TileSchema.Id.None && (effect.Tags == null || effect.Tags.Count == 0))
                        {
                            continue;
                        }
                        
                        player.AddModDamageTaken(effect);
                        break;
                    
                    case EffectType.ModXp:
                        if (effect.Id == TileSchema.Id.None && (effect.Tags == null || effect.Tags.Count == 0))
                        {
                            continue;
                        }

                        player.AddModXp(effect);
                        break;
                    
                    case EffectType.BonusSpawn:
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.AddSpawnCount(effect.Id, effect.Amount);
                        }
                        // TODO: Support by Tag?
                        break;
                    
                    case EffectType.SwapTiles:
                        if (effect.OtherId != TileSchema.Id.None && effect.Tags != null && effect.Tags.Count > 0)
                        {
                            player.AddTileSwapByTagEntry(effect.Tags[0], effect.OtherId, effect.Amount);
                        }

                        if (effect.Id != TileSchema.Id.None && effect.OtherId != TileSchema.Id.None)
                        {
                            player.AddTileSwapEntry(effect.Id, effect.OtherId, effect.Amount);
                        }
                        
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
                        ServiceLocator.Instance.AudioManager.PlaySfx("HealthPayment");
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
                    case EffectType.AddRandomItem:
                        for (int i = 0; i < effect.Amount; i++)
                        {
                            var item = effect.Items.GetRandomItem();
                            var granted = ServiceLocator.Instance.Player.Inventory.AddItem(item, effect.GrantItemForceAllowDuplicates);
                            if (granted != null)
                            {
                                GrantedItems.Add(granted);
                            }
                        }
                        break;
                    case EffectType.InstantReveal:
                        for (int i = 0; i < effect.Amount; i++)
                        {
                            if (effect.Id != TileSchema.Id.None)
                            {
                                // TODO: Provide VFX
                                ServiceLocator.Instance.Grid.RevealRandomOfType(effect.Id, 
                                    null, 
                                    new List<Tile.TileState>() { Tile.TileState.Hidden }
                                );
                            }
                            else if (effect.Tags.Count > 0)
                            {
                                // TODO: Provide VFX
                                ServiceLocator.Instance.Grid.RevealRandomOfTag(
                                    effect.Tags.GetRandomItem(), 
                                    null, 
                                    new List<Tile.TileState>() { Tile.TileState.Hidden }
                                );
                            }
                        }
                        break;
                    case EffectType.MassTeleport:
                        ServiceLocator.Instance.Grid.MassTeleport(effect.Tags);
                        break;
                    
                    case EffectType.InstantXP:
                        ServiceLocator.Instance.Player.TEMP_UpdateXP(null, effect.Amount);
                        break;
                    
                    case EffectType.InstantRevealRandomCol:
                        ServiceLocator.Instance.Grid.RevealRandomRow();
                        break;
                    
                    case EffectType.ModXpCurve:
                        ServiceLocator.Instance.Player.ModXpCurve += effect.Amount;
                        break;
                    
                    case EffectType.AddRandomItemByDungeonLevel:
                        Rarity rarity = Rarity.Common;
                        switch (ServiceLocator.Instance.LevelManager.CurrentLevel)
                        {
                            case 0:
                                rarity = Rarity.Uncommon;
                                break;
                            case 1:
                                rarity = Rarity.Rare;
                                break;
                            case 2:
                                rarity = Rarity.Epic;
                                break;
                            case 3:
                            case 4:
                                rarity = Rarity.Legendary;
                                break;
                        }
                        
                        var itemsOfRarity = ServiceLocator.Instance.Schemas.ItemSchemas.FindAll(x => x.Rarity == rarity);
                        ItemInstance itemGranted = ServiceLocator.Instance.Player.Inventory.AddItem(itemsOfRarity.GetRandomItem().ItemId, effect.GrantItemForceAllowDuplicates);
                        if (itemGranted != null)
                        {
                            GrantedItems.Add(itemGranted);
                        }
                        
                        break;
                        
                    case EffectType.MassPolymorph:
                        ServiceLocator.Instance.Grid.MassPolymorph(effect.Id);
                        break;
                    
                    case EffectType.InstantConquer:
                        for (int i = 0; i < effect.Amount; i++)
                        {
                            if (effect.Id != TileSchema.Id.None)
                            {
                                ServiceLocator.Instance.Grid.ConquerRandomOfType(effect.Id);
                            }
                            else if (effect.Tags.Count > 0)
                            {
                                ServiceLocator.Instance.Grid.ConquerRandomOfTag(effect.Tags.GetRandomItem());
                            }
                        }
                        break;
                    
                    case EffectType.Shield:
                        ServiceLocator.Instance.Player.AddShield(effect.Amount);
                        break;
                    
                }
            }
        }
        
        // TODO: Hack -- we are using this to "undo" anything an item from a chest would give when you retry
        public void UndoEffect(Player player, EffectTrigger trigger)
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
                    // These don't persist through player reset, so we're ok
                    case EffectType.ModXp:
                    case EffectType.Damage:
                    case EffectType.Heal:
                    case EffectType.RevealRandomLocation:
                    case EffectType.InstantReveal:
                    case EffectType.MassTeleport:
                    case EffectType.InstantXP:
                    case EffectType.InstantRevealRandomCol:
                    case EffectType.MassPolymorph:
                    case EffectType.InstantConquer:
                    case EffectType.Shield:
                        break;

                    case EffectType.ModDamageTaken:
                        player.UndoModDamageTaken(effect);
                        break;

                    case EffectType.BonusHP:
                        player.AddBonusStartHp(-effect.Amount);
                        break;
                    
                    case EffectType.BonusXP:
                        player.AddBonusStartXp(-effect.Amount);
                        break;
                    
                    case EffectType.AutoReveal:
                        for (int i = 0; i < effect.Amount; i++)
                        {
                            if (effect.Id != TileSchema.Id.None)
                            {
                                player.RemoveMonsterFromAutoRevealedList(effect.Id);
                            }

                            if (effect.Tags.Count > 0)
                            {
                                foreach (var effectTag in effect.Tags)
                                {
                                    player.RemoveMonsterToAutoRevealedByTagList(effectTag);
                                }
                            }
                        }
                        break;
                    
                    case EffectType.BonusSpawn:
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.AddSpawnCount(effect.Id, -effect.Amount);
                        }
                        // TODO: Support by Tag?
                        break;
                    
                    case EffectType.SwapTiles:
                        if (effect.OtherId != TileSchema.Id.None && effect.Tags != null && effect.Tags.Count > 0)
                        {
                            player.AddTileSwapByTagEntry(effect.Tags[0], effect.OtherId, -effect.Amount);
                        }

                        if (effect.Id != TileSchema.Id.None && effect.OtherId != TileSchema.Id.None)
                        {
                            player.AddTileSwapEntry(effect.Id, effect.OtherId, -effect.Amount);
                        }
                        
                        break;
                    
                    case EffectType.UpgradeTileObject:
                        if (effect.Id != TileSchema.Id.None)
                        {
                            player.DecrementTileLevel(effect.Id);
                        }
                        // TODO: Support by Tag?
                        break;
                    
                    case EffectType.ChangeMoney:
                        player.ShopXp -= effect.Amount;
                        break;
                    
                    case EffectType.AddRandomItem:
                    case EffectType.AddRandomItemByDungeonLevel:
                        RemoveGrantedItems();
                        break;
                    
                    case EffectType.ModXpCurve:
                        ServiceLocator.Instance.Player.ModXpCurve -= effect.Amount;
                        break;
                }
            }
        }

        public void RemoveGrantedItems()
        {
            foreach (var itemInstance in GrantedItems)
            {
                ServiceLocator.Instance.Player.Inventory.RemoveItem(itemInstance);
            }
            GrantedItems.Clear();
        }
        
        public void ReplenishAllCharges()
        {
            CurrentCharges = MaxCharges;
        }

        public void KeepGrantedItems()
        {
            GrantedItems.Clear();
        }
    }
}