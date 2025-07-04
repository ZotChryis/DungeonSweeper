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
        }

        [Serializable]
        public enum TriggerType
        {
            Victory,
            Pacifist,
            FullBoardClear,
        }
        
        
        public Id AchievementId;

        public string Title;
        public string Description;
        public string Reward;
        
        /// <summary>
        /// The class that is unlocked through getting this Achievement.
        /// </summary>
        public Class.Id RewardClass;
        
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

    }
}