using System;
using System.Collections.Generic;
using Schemas;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnSettings")]
public class SpawnSettings : Schema
{
    public string LabelText;
    
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

    private void OnValidate()
    {
        Queue<TileSchema> toCheck = new Queue<TileSchema>();
        int runningXP = 0;
        foreach (var entry in GridSpawns)
        {
            for (int i = 0; i < entry.Amount; i++)
            {
                toCheck.Enqueue(entry.Object);
                if (entry.ConsecutiveSpawn != null)
                {
                    for (int j = 0; j < entry.ConsecutiveCopies; j++)
                    {
                        toCheck.Enqueue(entry.ConsecutiveSpawn);
                    }
                }
            }
        }

        foreach (var entry in NormalSpawns)
        {
            for (int i = 0; i < entry.Amount; i++)
            {
                toCheck.Enqueue(entry.Object);
            }
        }

        while (toCheck.Count > 0)
        {
            var entry = toCheck.Dequeue();
            if (entry == null)
            {
                continue;
            }

            // Don't count dragon, cause that's not fair lol
            if (entry.TileId == TileSchema.Id.Dragon)
            {
                continue;
            }
            
            runningXP += entry.XPReward;
            
            if (entry.SpawnsFleeingChild && entry.FleeingChild != null)
            {
                toCheck.Enqueue(entry.FleeingChild);
            }
        }
        
        
        LabelText = "Max XP Available Before Dragon: " + runningXP;
    }
}