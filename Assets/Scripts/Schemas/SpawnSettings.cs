using System;
using Schemas;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnSettings")]
public class SpawnSettings : Schema
{
    [Serializable]
    public struct GridSpawnEntry
    {
        public int Amount;
        public TileSchema Object;
        
        /// <summary>
        /// Spawn this immediately following the Object spawn
        /// </summary>
        public TileSchema ConsecutiveSpawn;
        public int ConsecutiveCopies;
        public bool ConsecutiveStackedInLibrary;
        
        /// <summary>
        /// For minotaur+chest.
        /// Minotaur is the additional spawn which look towards the spawner.
        /// </summary>
        public bool GuardRelationship;
        /// <summary>
        /// For gargoyles. Look towards each other orthogonally.
        /// </summary>
        public bool LookTowardsEachOther;
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