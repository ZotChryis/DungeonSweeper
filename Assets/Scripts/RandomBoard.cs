using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents a 2d board that fetches x, y coordinates.
/// </summary>
public class RandomBoard
{
    private List<int> UnoccupiedSpaces = new List<int>();
    
    private float width;

    /// <summary>
    /// Height of board.
    /// </summary>
    private float height;

    /// <summary>
    /// Creates a board.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public RandomBoard(int width, int height)
    {
        this.width = (float)width;
        this.height = (float)height;
        int totalSize = width * height;
        for (int i = 0; i < totalSize; i++)
        {
            UnoccupiedSpaces.Add(i);
        }
    }

    public (int, int) GetAndRemoveRandomUnoccuppiedSpace()
    {
        int DELETEME = Random.Range(0, UnoccupiedSpaces.Count);
        Debug.Log("RandomBoard random flat coordinate: " + DELETEME + " , and unoccupiedSpacesCount: " + UnoccupiedSpaces.Count);
        int flatCoordinate = UnoccupiedSpaces[DELETEME];
        (int x, int y) = FlatToCoordinate(flatCoordinate);
        if(!RemoveUnoccupiedSpace(x, y))
        {
            Debug.LogWarning("Failed to remove random x:" + x +  ",y:" + y);
        }
        return (x, y);
    }

    /// <summary>
    /// Sets a tile space to occupied by removing it from the board.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool RemoveUnoccupiedSpace(int x, int y)
    {
        return UnoccupiedSpaces.Remove(CoordinateToFlat(x, y));
    }

    private int CoordinateToFlat(int x, int y)
    {
        return (y) * (int)this.width + x;
    }

    private (int, int) FlatToCoordinate(int flatCoordinate)
    {
        int x = (flatCoordinate) % (int)this.width;
        int y = Mathf.FloorToInt((flatCoordinate) / this.width);
        return (x, y);
    }
}
