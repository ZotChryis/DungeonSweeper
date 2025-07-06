using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Schemas
{
    /// <summary>
    /// The shop will care about having rarity.
    /// Some items might increase rarity given in the shop.
    /// </summary>
    [Serializable]
    public enum Rarity
    {
        Common,     // 100% chance to appear in the shop
        Uncommon,   // 75% chance to appear in the shop
        Rare,       // 50% chance to appear in the shop
        Epic,       // 25% chance to appear in the shop
        Legendary,  // 5% chance to appear in the shop
    }

    [Serializable]
    public enum EffectType
    {
        BonusHP,
        BonusXP,
        AutoReveal,
        ModDamageTaken,
        ModXp,
        BonusSpawn,
        UpgradeTileObject,
        Damage,
        Heal,
        ChangeMoney,
        RevealRandomLocation
    }

    [Serializable]
    public enum EffectTrigger
    {
        Purchase,       // When the item is purchased
        PlayerLevel,    // When the player levels up mid-dungeon round
        DungeonLevel,   // When the dungeon levels up (Victory screen)
        Conquer,        // When anything is conquered 
        Used,           // Consumables only: When the Use button is clicked in inventory
    }

    [Serializable]
    public enum DecayTrigger
    {
        PlayerLevel,
        DungeonLevel,
        Conquer,
    }
    
    [Serializable]
    public struct Effect
    {
        public EffectType Type;
        
        /// <summary>
        /// If the effect type needs an amount, this will be used.
        /// </summary>
        public int Amount;
        
        /// <summary>
        /// The effect will only apply if the object in question has this id
        /// </summary>
        public TileSchema.Id Id;

        /// <summary>
        /// The effect will only apply if at least one of these tags are on the object in question
        /// </summary>
        public List<TileSchema.Tag> Tags;

        /// <summary>
        /// How many 'turns' this effect lasts. Use -1 for 'forever'.
        /// TODO: Currently only supported by ModDamage but we should support all effects
        /// </summary>
        public int Decay;
        
        /// <summary>
        /// What causes the Decay to decrement.
        /// </summary>
        public DecayTrigger DecayTrigger;

        /// <summary>
        /// Some decay triggers need tile tags to filter. Use these
        /// </summary>
        public List<TileSchema.Tag> DecayTags;
        
        // TODO:
        /*
        /// <summary>
        /// The effect will only apply if the object in question has this id
        /// </summary>
        // TODO: This maps to monster id, etc, but we should really be using enums and shit??
        public TileSchema.Id ConquerRequirementId;

        /// <summary>
        /// The effect will only apply if at least one of these tags are on the object in question
        /// </summary>
        public List<TileSchema.Tag> ConquerRequirementTags;
        */
    }
    
    [CreateAssetMenu(menuName = "Data/Item")]
    public class ItemSchema : Schema
    {
        private void OnValidate()
        {
            if (ItemId == Id.None)
            {
                Debug.LogError($"{nameof(ItemSchema)}.{Name} requires a valid item ID");
            }
        }
        
        // !!WARNING!! DO NOT REORDER
        public enum Id
        {
            // Used for Empty
            None,
            
            Sword,
            Bow,
            MagicCarpet,
            Flute,
            TarotDeck,
            SpellHolyLight,
            Alembic,
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
            Pickaxe,
            
            PotionHealing,
            PotionStrength,
            PotionPoison,
            PotionStamina,
            PotionStoneshield,
            
            Lantern,
        }
        
        public ItemSchema.Id ItemId;
        public string Name;
        public string Description;
        public Sprite Sprite;
        public bool IsUniqueEquipped;
        
        // Consumable only
        public bool IsConsumbale;
        public int InitialCharges;

        /// <summary>
        /// The amount spawned in the shop when the item is rolled.
        /// </summary>
        public int ShopInventory = 1;
        
        /// <summary>
        /// Determines how often you will see this in the shop.
        /// </summary>
        public Rarity Rarity;
        
        /// <summary>
        /// How much "ShopXP" it will cost to buy one instance.
        /// </summary>
        public int Price;
        
        [SerializedDictionary("EffectTrigger", "Effects")]
        public SerializedDictionary<EffectTrigger, Effect[]> Effects =  new SerializedDictionary<EffectTrigger, Effect[]>();
        
        public float GetShopAppearanceRate()
        {
            switch (Rarity)
            {
                case Rarity.Common:
                    return 1.0f;
                case Rarity.Uncommon:
                    return 0.75f;
                case Rarity.Rare:
                    return 0.5f;
                case Rarity.Epic:
                    return 0.25f;
                case Rarity.Legendary:
                    return 0.05f;
            }

            return 1.0f;
        }
    }
}