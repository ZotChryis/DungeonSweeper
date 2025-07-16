using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnRequirement : Schema
{
    [Serializable]
    public struct RequirementOption
    {
        public bool UseRequirement;
        public bool Value;
    }
    
    [SerializeField] protected bool Negate;

    public bool RevealAfterSpawn = false;

    protected List<(int, int)> CoordinateList = new List<(int, int)>();

    
    /// <summary>
    /// Removes from CoordinateList all spots that are occupied by a tile.
    /// Requires you to pass in the RandomBoard tracker.
    /// </summary>
    // TODO: Uniformly run this as a setting?
    public void RemoveOccupiedSpaces(RandomBoard board)
    {
        for (var i = CoordinateList.Count - 1; i >= 0 ; i--)
        {
            var  coord = CoordinateList[i];
            if (!board.PeekUnoccupiedSpace(coord.Item1, coord.Item2))
            {
                Debug.Log("EdgeSpawnRequirement Filtered out valid coord because it was full: " + coord.Item1 + " , " + coord.Item2);
                CoordinateList.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Returns a random valid spawn location.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public abstract (int x, int y) GetRandomCoordinate(RandomBoard board);

    public abstract List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY);
}