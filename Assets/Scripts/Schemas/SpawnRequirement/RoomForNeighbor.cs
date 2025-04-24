using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn at random spot but also has adjacent empty space.
/// </summary>
[CreateAssetMenu(menuName = "Data/SpawnRequirement/RoomForNeighbor")]
public class RoomForNeighbor : SpawnRequirement
{
    /// <summary>
    /// Number of spaces you need to be valid.
    /// </summary>
    public int numberOfValidAdjacentSpaces;

    /// <summary>
    /// Potential spaces that should be adjacent.
    /// </summary>
    public CompassDirections[] EmptyAdjacentSpaces;

    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        CoordinateList.Clear();
        foreach (var adjacentSpace in EmptyAdjacentSpaces)
        {
            (int checkX, int checkY) = adjacentSpace.GetCompassTilePos(inputX, inputY);
            if (board.PeekUnoccupiedSpace(checkX, checkY))
            {
                CoordinateList.Add((checkX, checkY));
            }
        }
        return CoordinateList;
    }

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        (int checkX, int checkY) = board.PeekUnoccupiedRandomSpace();
        for (int i = 0; i < board.width * board.height; i++)
        {
            if (IsValid(checkX, checkY, board))
            {
                return (checkX, checkY);
            }
            else
            {
                (checkX, checkY) = board.GetNextUnoccupiedSpace(checkX, checkY);
            }
        }
        Debug.Log("Somehow did not find room for neighbor!");
        return (checkX, checkY);
    }

    /// <summary>
    /// For RoomForNeighbor, we check the given position has enough adjacent empty spaces.
    /// </summary>
    /// <param name="xCoord"></param>
    /// <param name="yCoord"></param>
    /// <param name="board"></param>
    /// <returns></returns>
    public bool IsValid(int xCoord, int yCoord, RandomBoard board)
    {
        CoordinateList.Clear();
        foreach (var adjacentSpace in EmptyAdjacentSpaces)
        {
            (int checkX, int checkY) = adjacentSpace.GetCompassTilePos(xCoord, yCoord);
            if (board.PeekUnoccupiedSpace(checkX, checkY))
            {
                CoordinateList.Add((checkX, checkY));
                if (CoordinateList.Count >= numberOfValidAdjacentSpaces)
                {
                    return true;
                }
            }
        }
        return CoordinateList.Count >= numberOfValidAdjacentSpaces;
    }
}
