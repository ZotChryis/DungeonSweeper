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
        public SpawnRequirement[] Requirements;
    }
    
    public int Height;
    public int Width;
    
    /// <summary>
    /// Ordered entries to spawn. They have their own spawn requirements.
    /// </summary>
    public GridSpawnEntry[] GridSpawns;

    /// <summary>
    /// Bosses to spawn.
    /// </summary>
    public GridSpawnEntry[] BossSpawns;
}