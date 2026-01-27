using System;
using System.Collections.Generic;
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
            MineCollector,  // Collect a diffused mine (unlocks Junker)
            
            Junker0,
            Junker1,
            Junker2,
            Junker3,
            Junker4,
            
            Challenges0,    // Complete 1 Challenge
            Challenges1,    // Complete 5 Challenges
            Challenges2,    // Complete X Challenges
            Challenges3,    // Complete X Challenges
            Challenges4,    // Complete X Challenges
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
            Challenges,
        }
        
        public Id AchievementId;

        public string Title;
        public string Description;
        public string Reward;
        public int SortGroup;

        /// <summary>
        /// Most (if not all) achievements are disabled in challenges. This lets us bypass that.
        /// </summary>
        public bool CanBeCompletedDuringChallenges = false;
        
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
        /// Some achievements require you to have a certain amount of challenges completed. This is that count.
        /// </summary>
        public int ChallengeCount = 0;

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
        
        // A programmatic way (since I was too lazy to do in data itself) to define achievement chains
        // The achievement screen will go through this first, THEN iterate over the rest of the schemas
        // to create the list. We use the sort order of the topmost of the chain when sorting them
        public static List<List<AchievementSchema.Id>> ChainedAchievements = new List<List<AchievementSchema.Id>>()
        {
            new  List<AchievementSchema.Id>()
            {
                Id.Adventurer0,
                Id.Adventurer1,
                Id.Adventurer2,
                Id.Adventurer3,
                Id.Adventurer4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Warrior0,
                Id.Warrior1,
                Id.Warrior2,
                Id.Warrior3,
                Id.Warrior4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Ranger0,
                Id.Ranger1,
                Id.Ranger2,
                Id.Ranger3,
                Id.Ranger4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Wizard0,
                Id.Wizard1,
                Id.Wizard2,
                Id.Wizard3,
                Id.Wizard4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Ritualist0,
                Id.Ritualist1,
                Id.Ritualist2,
                Id.Ritualist3,
                Id.Ritualist4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Bard0,
                Id.Bard1,
                Id.Bard2,
                Id.Bard3,
                Id.Bard4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.FortuneTeller0,
                Id.FortuneTeller1,
                Id.FortuneTeller2,
                Id.FortuneTeller3,
                Id.FortuneTeller4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Miner0,
                Id.Miner1,
                Id.Miner2,
                Id.Miner3,
                Id.Miner4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Priest0,
                Id.Priest1,
                Id.Priest2,
                Id.Priest3,
                Id.Priest4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Apothecary0,
                Id.Apothecary1,
                Id.Apothecary2,
                Id.Apothecary3,
                Id.Apothecary4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Gambler0,
                Id.Gambler1,
                Id.Gambler2,
                Id.Gambler3,
                Id.Gambler4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Merchant0,
                Id.Merchant1,
                Id.Merchant2,
                Id.Merchant3,
                Id.Merchant4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Scribe0,
                Id.Scribe1,
                Id.Scribe2,
                Id.Scribe3,
                Id.Scribe4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Dryad0,
                Id.Dryad1,
                Id.Dryad2,
                Id.Dryad3,
                Id.Dryad4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.ItemAscetic0,
                Id.ItemAscetic1,
                Id.ItemAscetic2,
                Id.ItemAscetic3,
                Id.ItemAscetic4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Aristocrat0,
                Id.Aristocrat1,
                Id.Aristocrat2,
                Id.Aristocrat3,
                Id.Aristocrat4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Assassin0,
                Id.Assassin1,
                Id.Assassin2,
                Id.Assassin3,
                Id.Assassin4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.BountyHunter0,
                Id.BountyHunter1,
                Id.BountyHunter2,
                Id.BountyHunter3,
                Id.BountyHunter4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Chef0,
                Id.Chef1,
                Id.Chef2,
                Id.Chef3,
                Id.Chef4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Beekeeper0,
                Id.Beekeeper1,
                Id.Beekeeper2,
                Id.Beekeeper3,
                Id.Beekeeper4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Junker0,
                Id.Junker1,
                Id.Junker2,
                Id.Junker3,
                Id.Junker4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.PacifistLovers0,
                Id.PacifistLovers1,
                Id.PacifistLovers2,
                Id.PacifistLovers3,
                Id.PacifistLovers4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Hardcore0,
                Id.Hardcore1,
                Id.Hardcore2,
                Id.Hardcore3,
                Id.Hardcore4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.PacifistRat0,
                Id.PacifistRat1,
                Id.PacifistRat2,
                Id.PacifistRat3,
                Id.PacifistRat4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Annihilation0,
                Id.Annihilation1,
                Id.Annihilation2,
                Id.Annihilation3,
                Id.Annihilation4
            },
            
            new  List<AchievementSchema.Id>()
            {
                Id.Challenges0,
                Id.Challenges1,
            },
        };
    }
}