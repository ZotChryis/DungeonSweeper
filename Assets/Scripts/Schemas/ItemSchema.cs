using System;
using Gameplay;
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
        Base,       // 100% chance to appear in the shop
        Common,     // 75% chance to appear in the shop
        Uncommon,   // 55% chance to appear in the shop
        Rare,       // 35% chance to appear in the shop
        Epic,       // 15% chance to appear in the shop
        Legendary,  // 5% chance to appear in the shop
    }
    
    [CreateAssetMenu(menuName = "Data/Item")]
    public class ItemSchema : Schema
    {
        public Item.Id Id;
        public string Name;
        public string Description;
        public Sprite Sprite;
        [FormerlySerializedAs("IsComsumbale")] public bool IsConsumbale;
        public int InitialCharges;
        
        public Rarity Rarity;
        public int Price;
        
        public float GetSuccessRate()
        {
            switch (Rarity)
            {
                case Rarity.Base:
                    return 1.0f;
                case Rarity.Common:
                    return 0.75f;
                case Rarity.Uncommon:
                    return 0.55f;
                case Rarity.Rare:
                    return 0.35f;
                case Rarity.Epic:
                    return 0.15f;
                case Rarity.Legendary:
                    return 0.05f;
            }

            return 0f;
        }
    }
}