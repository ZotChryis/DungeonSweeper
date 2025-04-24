using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn at random spot but also has adjacent empty space to the right.
/// </summary>
[CreateAssetMenu(menuName = "Data/SpawnRequirement/RoomForRow")]
public class RoomForRow : SpawnRequirement
{
    /// <summary>
    /// Min distance to the right unoccupied space must be.
    /// </summary>
    public int AdjacencyAllowed;

    /// <summary>
    /// Distance the primary spawn must be from the right edge.
    /// This might be unnecessary and constantly equal to the adjacency allowed...
    /// </summary>
    public int RightMargin = 0;

    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        CoordinateList.Clear();
        for (int x = inputX + AdjacencyAllowed; x < board.width; x++)
        {
            if(board.PeekUnoccupiedSpace(x, inputY))
            {
                CoordinateList.Add((x, inputY));
            }
        }
        return CoordinateList;
    }

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        (int x, int y) = board.PeekUnoccupiedRandomSpaceWithMargins(0, RightMargin, 0, 0);
        if (IsValid(x, y, board))
        {
            return (x, y);
        }
        for (int i = 0; i < board.LoopCount; i++)
        {
            (x, y) = board.GetNextUnoccupiedSpace(x, y);
            if (IsValid(x, y, board))
            {
                return (x, y);
            }
        }
        Debug.Log("Room for row did not find a valid unoccupied spot.");
        return (x, y);
    }

    public bool IsValid(int xCoord, int yCoord, RandomBoard board)
    {
        (int retX, int retY) = (xCoord + AdjacencyAllowed, yCoord);
        for (int i = retX; i < board.width; i++)
        {
            if (board.PeekUnoccupiedSpace(retX, retY))
            {
                return true;
            }
        }
        return false;
    }
}
