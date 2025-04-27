using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn children randomly without impacting the primary spawn.
/// For easier implementation of SpawnRequirement.
/// </summary>
public abstract class NoCheckChildSpawnRequirement : SpawnRequirement
{
    [SerializeField]
    private CompassDirections[] PotentialSpawnLocations;

    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        CoordinateList.Clear();
        foreach (var direction in PotentialSpawnLocations)
        {
            (int spawnX, int spawnY) = direction.GetCompassTilePos(inputX, inputY);
            if (board.PeekUnoccupiedSpace(spawnX, spawnY))
            {
                CoordinateList.Add((spawnX, spawnY));
            }
        }
        return CoordinateList;
    }
}
