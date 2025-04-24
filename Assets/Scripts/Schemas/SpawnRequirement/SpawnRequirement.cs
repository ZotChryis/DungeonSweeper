using NUnit.Framework;
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

    protected List<(int, int)> CoordinateList = new List<(int, int)>();

    // not really needed?
    //public abstract bool IsValid(int xCoord, int yCoord, RandomBoard board);

    /// <summary>
    /// Returns a random valid spawn location.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public abstract (int x, int y) GetRandomCoordinate(RandomBoard board);

    public abstract List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY);
}