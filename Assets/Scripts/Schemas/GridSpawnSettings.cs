using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GridSpawnSettings")]
public class GridSpawnSettings : Schema
{
    [Serializable]
    public struct GridSpawnEntry
    {
        public int Amount;
        public TileObjectSchema Object;
        public GridSpawnRequirement[] Requirements;
    }
    
    /// <summary>
    /// Ordered entries to spawn. They have their own spawn requirements.
    /// </summary>
    public GridSpawnEntry[] GridSpawns;
}