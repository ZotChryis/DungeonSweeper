using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Schemas
{
    [CreateAssetMenu(menuName = "Data/Challenge")]
    public class ChallengeSchema : Schema
    {
        // !!!! DO NOT RE-ORDER !!!!!
        // It's okay to have some as unused if needed
        [Serializable]
        public enum Id
        {
            None,                   // Used for "empty" state
            UltimateWizard,         // Defeat the last 2 levels as Wizard with ALL spells + Spellbook
            HighLowHigh,            // XP curve goes 10->4->11
            PotionOverdose,         // Apothecary + all potions, NO SHOP
            Flagellation,           // Start the game with x3 Sacraficial Whips and some Holy Spells
            GaiasRevenge,           // Start with Amulets & Rings, Greater Golem, and Clamancy. NO SHOP. 
        }

        public Id ChallengeId;
        
        // Shown in the challenge screen and in toasts
        public string Title;
        
        // Shown in the challenge screen
        public string Context;
        
        // If not NONE, then the challenge HAS to be done with that class
        public Class.Id StartingClass = Class.Id.None;

        // These classes are BLOCKED from being used during this challenge.
        // By default, we disable Aristocrat because its a "newbie friendly" class
        // Also remove Ascetic because items
        public List<Class.Id> BlockedClasses = new() { Class.Id.Aristocrat, Class.Id.Ascetic };
        
        // Any extra items to be added to the player for this challenge
        public ItemSchema[] StartingItems;
        
        // Instruct the challenge to override the levels in LevelManager during this challenge
        public bool OverrideLevels;
        public SpawnSettings[] Levels;
        
        // Instruct the challenge to override the player level progression during this challenge
        public bool OverrideLevelProgression;
        public LevelProgressionSchema LevelProgression;

        // If true, will skip the shop when in victory screen
        public bool BlockShopping = false;
    }
}