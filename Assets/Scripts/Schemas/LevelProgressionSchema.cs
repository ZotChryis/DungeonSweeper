using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Representation of level progression for the player.
/// </summary>
[CreateAssetMenu(menuName = "Data/LevelProgression")]
public class LevelProgressionSchema : Schema
{
    [Serializable]
    public struct LevelProgressionEntry
    {
        public int XPRequiredToLevel;
        public int MaxHealth;
        public string LabelText;
    }
    
    [SerializeField] 
    private LevelProgressionEntry[] LevelProgressionEntries;

    /// <summary>
    /// Helper function to get the XP Requirement to level for the given level.
    /// We assume players start at level 1.
    /// </summary>
    public int GetXPRequiredForLevel(int level)
    {
        int adjustedIndex = level - 1;
        if (adjustedIndex < 0)
        {
            return 0;
        }

        if (adjustedIndex >= LevelProgressionEntries.Length)
        {
            return LevelProgressionEntries[^1].XPRequiredToLevel;
        }
        
        return LevelProgressionEntries[adjustedIndex].XPRequiredToLevel;
    }
    
    /// <summary>
    /// Helper function to get the Max Health for the given level.
    /// We assume players start at level 1.
    /// </summary>
    public int GetMaxHealthForLevel(int level)
    {
        int adjustedIndex = level - 1;
        if (adjustedIndex < 0)
        {
            return 0;
        }

        if (adjustedIndex >= LevelProgressionEntries.Length)
        {
            return LevelProgressionEntries[^1].MaxHealth;
        }
        
        return LevelProgressionEntries[adjustedIndex].MaxHealth;
    }

    private void OnValidate()
    {
        if (LevelProgressionEntries == null)
        {
            return;
        }

        int currentSum = 0;
        for (var i = 0; i < LevelProgressionEntries.Length; i++)
        {
            currentSum += LevelProgressionEntries[i].XPRequiredToLevel;
            LevelProgressionEntries[i].LabelText = "XP Needed to get to this level: " + currentSum;
        }
    }
}
