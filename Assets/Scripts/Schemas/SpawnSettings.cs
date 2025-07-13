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
        /// Minotaur is the additional spawn which enrages when one is conquered.
        /// </summary>
        public bool GuardRelationship;
        /// <summary>
        /// For gargoyles. Look towards each other orthogonally or away if false.
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
        Queue<(TileSchema, int)> toCheck = new Queue<(TileSchema, int)>();
        int runningXP = 0;
        foreach (var entry in GridSpawns)
        {
            toCheck.Enqueue((entry.Object, entry.Amount));
            if (entry.ConsecutiveSpawn != null)
            {
                toCheck.Enqueue((entry.ConsecutiveSpawn, entry.ConsecutiveCopies * entry.Amount));
            }
        }

        foreach (var entry in NormalSpawns)
        {
            toCheck.Enqueue((entry.Object, entry.Amount));
        }

        while (toCheck.Count > 0)
        {
            var entryAndAmount = toCheck.Dequeue();
            var entry = entryAndAmount.Item1;
            if (entry == null)
            {
                continue;
            }

            // Don't count dragon, cause that's not fair lol
            if (entry.TileId == TileSchema.Id.Dragon)
            {
                continue;
            }

            // Don't count enemies that can't be killed
            if (entry.Power >= 20 && entry.Power % 100 != 0)
            {
                continue;
            }

            runningXP += entry.XPReward * entryAndAmount.Item2;

            if (entry.SpawnsFleeingChild && entry.FleeingChild != null)
            {
                toCheck.Enqueue((entry.FleeingChild, entryAndAmount.Item2));
            }
        }

        LabelText = "Max XP Available Before Dragon: " + runningXP;
    }
}