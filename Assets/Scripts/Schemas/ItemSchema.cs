using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
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
        AutoReveal,     // at the start of dungeons
        ModDamageTaken,
        ModXp,
        BonusSpawn,
        UpgradeTileObject,
        Damage,
        Heal,
        ChangeMoney,
        RevealRandomLocation,
        AddRandomItem,
        InstantReveal,  // Reveals a random when triggered
        MassTeleport,
        InstantXP,
        InstantRevealRandomCol,
        ModXpCurve,
        AddRandomItemByDungeonLevel,
        MassPolymorph,  // All revealed enemies are polymorphed
        InstantConquer, // Conquers a random when triggered
        SwapTiles,      // HACK: Takes all matching tiles using Tags[0], and swaps them to the TileId
        
    }

    [Serializable]
    public enum EffectTrigger
    {
        Purchase,       // When the item is purchased
        PlayerLevel,    // When the player levels up mid-dungeon round
        DungeonLevel,   // When the dungeon levels up (Victory screen)
        Conquer,        // When anything is conquered 
        Used,           // Consumables only: When the Use button is clicked in inventory
        Heal,           // When the player heals mid-dungeon 
    }

    [Serializable]
    public enum DecayTrigger
    {
        PlayerLevel,        // not implemented yet
        DungeonLevel,       // not implemented yet
        Conquer,
    }
    
    [Serializable]
    public class Effect
    {
        public EffectType Type;
        
        /// <summary>
        /// If the effect type needs an amount, this will be used.
        /// </summary>
        public int Amount;

        /// <summary>
        /// Items in the inventory abide by "max" amount, but this bool will overwrite that and allow an item to be
        /// added beyond max.
        /// </summary>
        public bool GrantItemForceAllowDuplicates;
        
        /// <summary>
        /// The effect will only apply if the object in question has this id
        /// </summary>
        public TileSchema.Id Id;

        /// <summary>
        /// The effect will only apply if at least one of these tags are on the object in question
        /// </summary>
        public List<TileSchema.Tag> Tags;

        /// <summary>
        /// Some effects can add items or filter off of them. Use these.
        /// </summary>
        public List<ItemSchema.Id> Items;
        
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
            SacrificialWhip,
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
            DetectorChest,
            SpellMassTeleportation,
            SlimeVacuum,
            
            CoinCopper,
            CoinSilver,
            CoinGold,
            
            EngagementRing,
            PrayerBeads,
            CommunicationsDegree,
            
            SpellPillarOfLight,
            
            PotionBloodrose,
            PotionAgility,
            
            RepellentRat,
            RepellentDemonKnight,
            
            AmuletTopaz,
            Dice,
            BusinessCard,
            
            PotionLove,
            TreasureMap,
            Stilts,
            Telescope,
            
            Quill,
            CreditCard,
            BaitVisionOrb,
            
            Spellbook,
            SpellPolymorph,
            SpellRainOfFire,
            
            BloodDonorCard,
            
            AmuletRuby,
            AmuletSapphire,
            AmuletEmerald,
            
            ScrimshawKit,
            PizzaBox,
            
            ForestDiadem,
            SpiritualTattoos,
            Wardstones,
            SpellNaturesSight,
            
        }
        
        public ItemSchema.Id ItemId;
        public string Name;
        public string Description;
        public Sprite Sprite;
        
        /// <summary>
        /// The amount of this item you can have in the inventory.
        /// -1 is infinite.
        /// </summary>
        public int Max = -1;
        
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

        /// <summary>
        /// Played when used. Empty will be ignored.
        /// </summary>
        public string UseSfx;
        
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