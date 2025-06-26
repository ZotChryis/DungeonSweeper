using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

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
        ModXp
    }

    [Serializable]
    public struct Effect
    {
        public EffectType Type;
        public int Amount;
        
        /// <summary>
        /// The effect will only apply if the object in question has this id
        /// </summary>
        // TODO: This maps to monster id, etc, but we should really be using enums and shit
        public string Id;

        /// <summary>
        /// The effect will only apply if these tags are on the object in question
        /// </summary>
        public List<TileObjectSchema.Tag> Tags;
    }
    
    [CreateAssetMenu(menuName = "Data/Item")]
    public class ItemSchema : Schema
    {
        public ItemInstance.Id Id;
        public string Name;
        public string Description;
        public Sprite Sprite;
        public bool IsUniqueEquipped;
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

        /// <summary>
        /// The gameplay effects that will run when buying this item.
        /// </summary>
        public Effect[] PassiveEffects;
        
        /// <summary>
        /// The gameplay effects that will run when actively used from inventory. Only CONSUMABLES should use this.
        /// </summary>
        public Effect[] ActiveEffects;
        
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