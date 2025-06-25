using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnSettings")]
public class SpawnSettings : Schema
{
    [Serializable]
    public struct GridSpawnEntry
    {
        public int Amount;
        public TileObjectSchema Object;
        
        /// <summary>
        /// Spawn this immediately following the Object spawn
        /// </summary>
        public TileObjectSchema ConsecutiveSpawn;
        public int ConsecutiveCopies;
        public bool ConsecutiveStackedInLibrary;
        
        public bool GuardRelationship;
        public SpawnRequirement Requirement;
    }
    
    public int Height;
    public int Width;
    
    /// <summary>
    /// Ordered entries to spawn. They have their own spawn requirements.
    /// </summary>
    public GridSpawnEntry[] GridSpawns;

    /// <summary>
    /// Normal entries to spawn. The order of these spawned should not matter.
    /// </summary>
    public GridSpawnEntry[] NormalSpawns;
}