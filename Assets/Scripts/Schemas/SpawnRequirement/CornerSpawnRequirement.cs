using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/Corner")]
public class CornerSpawnRequirement : SpawnRequirement
{
    [Serializable]
    public enum Corner
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight
    }

    /// <summary>
    /// Boolean OR requirements.
    /// Any pass will trigger a success.
    /// </summary>
    [SerializeField] 
    private Corner[] Requirements;

    /*public bool IsValid(int xCoord, int yCoord, RandomBoard board)
    {
        Grid grid = ServiceLocator.Instance.Grid;
        foreach (Corner edge in Requirements)
        {
            if (edge == Corner.BottomLeft && xCoord == 0 && yCoord == 0)
            {
                return !Negate;
            }

            if (edge == Corner.BottomRight && xCoord == grid.GetWidth() - 1 && yCoord == 0)
            {
                return !Negate;
            }

            if (edge == Corner.TopLeft && xCoord == 0 && yCoord == grid.GetHeight() - 1)
            {
                return !Negate;
            }

            if (edge == Corner.TopRight && xCoord == grid.GetWidth() - 1 && yCoord == grid.GetHeight() - 1)
            {
                return !Negate;
            }
        }

        return Negate;
    }*/

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        CoordinateList.Clear();
        CoordinateList.Add((0, 0));
        CoordinateList.Add((0, board.height - 1));
        CoordinateList.Add((board.width - 1, 0));
        CoordinateList.Add((board.width - 1, board.height - 1));
        return CoordinateList.GetRandomItem();
    }

    /// <summary>
    /// Corner does not support consecutive neighbors.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="inputX"></param>
    /// <param name="inputY"></param>
    /// <returns></returns>
    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        return new List<(int x, int y)>(0);
    }
}
