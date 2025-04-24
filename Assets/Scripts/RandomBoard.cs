using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a 2d board that fetches x, y coordinates.
/// Bottom left corner is 0,0. x is left to right. y is height.
/// </summary>
public class RandomBoard
{
    private List<int> UnoccupiedSpaces;

    public int width
    {
        get; private set;
    }

    /// <summary>
    /// Height of board.
    /// </summary>
    public int height
    {
        get; private set;
    }

    public int LoopCount
    {
        get
        {
            return (int)(width * height);
        }
    }

    /// <summary>
    /// Creates a board.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public RandomBoard(int width, int height)
    {
        this.width = width;
        this.height = height;
        int totalSize = width * height;
        UnoccupiedSpaces = new List<int>(totalSize);
        for (int i = 0; i < totalSize; i++)
        {
            UnoccupiedSpaces.Add(i);
        }
    }

    /// <summary>
    /// Gets the coordinates of a random empty grid location without marking it as used.
    /// </summary>
    public (int, int) PeekUnoccupiedRandomSpace()
    {
        return FlatToCoordinate(UnoccupiedSpaces[Random.Range(0, UnoccupiedSpaces.Count)]);
    }

    public (int, int) PeekUnoccupiedRandomSpaceWithMargins(int leftMargin, int rightMargin, int topMargin, int bottomMargin)
    {
        (int checkX, int checkY) = PeekUnoccupiedRandomSpace();
        return GetNextUnoccupiedSpaceWithMargin(leftMargin, rightMargin, topMargin, bottomMargin, checkX, checkY);
    }

    /// <summary>
    /// Returns the input coordinates if outside the margin.
    /// Else iterates until finds a coordinate outside the margin and unoccupied.
    /// Assumes the input coordinate is unoccupied.
    /// </summary>
    /// <param name="leftMargin"></param>
    /// <param name="rightMargin"></param>
    /// <param name="topMargin"></param>
    /// <param name="bottomMargin"></param>
    /// <param name="checkX"></param>
    /// <param name="checkY"></param>
    /// <returns></returns>
    public (int, int) GetNextUnoccupiedSpaceWithMargin(int leftMargin, int rightMargin, int topMargin, int bottomMargin, int checkX, int checkY)
    {
        for (int i = 0; i < LoopCount; i++)
        {
            // if inside a margin continue
            if (checkX < leftMargin || width - checkX <= rightMargin || checkY < bottomMargin || height - checkY <= topMargin)
            {
                (checkX, checkY) = GetNextUnoccupiedSpace(checkX, checkY);
                continue;
            }
            else
            {
                return (checkX, checkY);
            }
        }
        Debug.Log("Somehow we did not find an unoccupied space with provided margins.");
        return (checkX, checkY);
    }

    /// <summary>
    /// Gets if the coordinate is on this board (is unoccupied).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>True if the x,y is unoccupied</returns>
    public bool PeekUnoccupiedSpace(int x, int y)
    {
        return IsSpaceOnBoard(x, y) && UnoccupiedSpaces.Contains(CoordinateToFlat(x, y));
    }

    public bool HasEmptySpace()
    {
        return UnoccupiedSpaces.Count > 0;
    }

    /// <summary>
    /// Given a coordinate get the next unoccupied space scanning right and up.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public (int, int) GetNextUnoccupiedSpace(int x, int y)
    {
        for (int loopCount = 0; loopCount < LoopCount; loopCount++)
        {
            (x, y) = GetNextSpace(x, y);
            if (PeekUnoccupiedSpace(x, y))
            {
                return (x, y);
            }
        }
        return (x, y);
    }

    /// <summary>
    /// Go right and up.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private (int, int) GetNextSpace(int x, int y)
    {
        x++;
        if (x >= width)
        {
            y++;
            x = 0;
        }
        if (y >= height)
        {
            y = 0;
            x = 0;
        }
        return (x, y);
    }

    /// <summary>
    /// Go left and down.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private (int, int) GetPrevSpace(int x, int y)
    {
        x--;
        if (x < 0)
        {
            y--;
            x = (int)width - 1;
        }
        if (y < 0)
        {
            x = (int)width - 1; y = (int)height - 1;
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

    private bool IsSpaceOnBoard(int x, int y)
    {
        if (x < 0 || y < 0) return false;

        return x < width && y < height;
    }

    private int CoordinateToFlat(int x, int y)
    {
        return (y) * this.width + x;
    }

    private (int, int) FlatToCoordinate(int flatCoordinate)
    {
        int x = (flatCoordinate) % (int)this.width;
        int y = Mathf.FloorToInt((flatCoordinate) / (float)this.width);
        return (x, y);
    }
}
