using System;
using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

namespace Schemas
{
    [CreateAssetMenu(menuName = "Data/Achievement")]
    public class AchievementSchema : Schema
    {
        // !!!WARNING: DO NOT REORDER/DELETE!!!!
        [Serializable]
        public enum Id
        {
            None,
            
            Adventurer0,
            Adventurer1,
            Adventurer2,
            Adventurer3,
            
            Warrior0,
            Warrior1,
            Warrior2,
            Warrior3,
            
            Ranger0,
            Ranger1,
            Ranger2,
            Ranger3,
            
            Wizard0,
            Wizard1,
            Wizard2,
            Wizard3,
            
            Bard0,
            Bard1,
            Bard2,
            Bard3,
            
            FortuneTeller0,
            FortuneTeller1,
            FortuneTeller2,
            FortuneTeller3,
            
            Miner0,
            Miner1,
            Miner2,
            Miner3,
            
            Ritualist0,
            Ritualist1,
            Ritualist2,
            Ritualist3,
            
            Completionist0,
            Completionist1,
            Completionist2,
            Completionist3,
            
            PacifistRat0,
            PacifistRat1,
            PacifistRat2,
            PacifistRat3,
            
            Annihilation0,
            Annihilation1,
            Annihilation2,
            Annihilation3,
            
            Priest0,
            Priest1,
            Priest2,
            Priest3,
            
            Apothecary0,
            Apothecary1,
            Apothecary2,
            Apothecary3,
            
            PacifistLovers0,
            PacifistLovers1,
            PacifistLovers2,
            PacifistLovers3,
            
            Merchant0,
            Merchant1,
            Merchant2,
            Merchant3,
            
            Gambler0,
            Gambler1,
            Gambler2,
            Gambler3,
            
            ItemAscetic0,
            ItemAscetic1,
            ItemAscetic2,
            ItemAscetic3,

            ItemHoarder0,
            ItemHoarder1,
            ItemHoarder2,
            ItemHoarder3,
            // Item? Get 4 Commons in 1 run?
            // Item? Get 3 Rares?
            // Item? Get 2 Epics?
            // Item? Get a Legendary?
            
            Scribe0,
            Scribe1,
            Scribe2,
            Scribe3,
            
            DemonLord,
            
            Dryad0,
            Dryad1,
            Dryad2,
            Dryad3,
            
            Hardcore0,
            Hardcore1,
            Hardcore2,
            Hardcore3,
            Hardcore4,
            
            KillerSheep,
            KillerVampire,
            KillerWerewolf,

            // Ultimate achievements for level 5. Made after level 5 was made so they are out of order of their
            // 0-3 counterparts
            Adventurer4,
            Warrior4,
            Ranger4,
            Wizard4,
            Bard4,
            FortuneTeller4,
            Miner4,
            Ritualist4,
            Completionist4,
            PacifistRat4,
            Annihilation4,
            Priest4,
            Apothecary4,
            PacifistLovers4,
            Merchant4,
            Gambler4,
            ItemAscetic4,
            Scribe4,
            Dryad4,
            
            
            Aristocrat0,
            Aristocrat1,
            Aristocrat2,
            Aristocrat3,
            Aristocrat4,
            
            Assassin0,
            Assassin1,
            Assassin2,
            Assassin3,
            Assassin4,
            
            BountyHunter0,
            BountyHunter1,
            BountyHunter2,
            BountyHunter3,
            BountyHunter4,

            DieAnyPercent,
            CrystalBallUser,
            HealScrollUser,
            MinotaurSlayer,
            GoldBugSlayer,
            BrickSlayer,
            LuckyShopper,
            ShopReroll,
            DemonKnightSlayer,
            PotionUser,
            FireLordSlayer,
            
            Chef0,
            Chef1,
            Chef2,
            Chef3,
            Chef4,
            
            Beekeeper0,
            Beekeeper1,
            Beekeeper2,
            Beekeeper3,
            Beekeeper4,
            
            PizzaUser,      // Eat a pizza slice (unlocks Chef)
            BeholderSlayer,   // Kill a Beholder (unlocks Beekeeper)
            
        }

        [Serializable]
        public enum TriggerType
        {
            Victory,
            Pacifist,
            FullBoardClear,
            DemonLord,
            Killer,
            None,
        }
        
        public Id AchievementId;

        public string Title;
        public string Description;
        public string Reward;
        public int SortGroup;
        
        /// <summary>
        /// Since some classes are paid exclusive, some achievements must also be.
        /// </summary>
        [FormerlySerializedAs("SteamExclusive")] public bool PaidExclusive;
        
        /// <summary>
        /// The class that is unlocked through getting this Achievement.
        /// </summary>
        public Class.Id RewardClass;
        
        /// <summary>
        /// The item that is unlocked through getting this Achievement.
        /// </summary>
        public ItemSchema.Id RewardItem;
        
        /// <summary>
        /// The trigger for the achievement to be checked.
        /// </summary>
        public TriggerType Trigger;

        /// <summary>
        /// Some achievements need extra int level to complete.
        /// Use -1 for "any".
        /// </summary>
        [FormerlySerializedAs("Value")] 
        public int Level = -1;

        /// <summary>
        /// Some achievements require a specific class.
        /// </summary>
        public Class.Id Class;
        
        /// <summary>
        /// Some achievements need extra tile data info.
        /// </summary>
        public TileSchema.Id[] TileData;
        
        /// <summary>
        /// Some achievements need extra item data info.
        /// </summary>
        public ItemSchema.Id[] ItemData;

        /// <summary>
        /// Some achievements require you to get items. -1 means any number of items.
        /// </summary>
        public int ItemAcquiredMinCount = -1;

        /// <summary>
        /// Some achievements require you to not get items. -1 means any number of items.
        /// </summary>
        public int ItemAcquiredMaxCount = -1;

        /// <summary>
        /// Some achievements require NO RETRY.
        /// </summary>
        public bool RequiresHardcore = false;

        /// <summary>
        /// KILLER achievements require a minimum kill count. This is that count.
        /// </summary>
        public int MinimumKillCount;

        /// <summary>
        /// Icon displayed in game and in steam.
        /// </summary>
        public Sprite AchievementIcon;
    }
}