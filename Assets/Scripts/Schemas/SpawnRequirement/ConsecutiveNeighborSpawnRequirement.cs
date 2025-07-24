using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pick one of the valid cardinal directions and ensure N amount of spots are valid in that direction.
/// </summary>
[CreateAssetMenu(menuName = "Data/SpawnRequirement/ConsecutiveNeighborSpawnRequirement")]
public class ConsecutiveNeighborSpawnRequirement : SpawnRequirement
{
    /// <summary>
    /// Number of spaces you need to be valid.
    /// </summary>
    public int numberOfValidSpaces;

    /// <summary>
    /// Potential directions that should be adjacent.
    /// </summary>
    public CompassDirections[] PossibleDirections;

    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        // Randomly choose the order to test in case multiple are valid
        PossibleDirections.Shuffle();
        
        foreach (var direction in PossibleDirections)
        {
            CoordinateList.Clear();
            
            (int checkX, int checkY) = (inputX, inputY);
            for (int i = 0; i < numberOfValidSpaces; i++)
            {
                (checkX, checkY) = direction.GetCompassTilePos(checkX, checkY);
                if (board.PeekUnoccupiedSpace(checkX, checkY))
                {
                    CoordinateList.Add((checkX, checkY));
                }
                else
                {
                    CoordinateList.Clear();
                    break;
                }
            }

            if (CoordinateList.Count == numberOfValidSpaces)
            {
                return CoordinateList;
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
    /// For RoomForNeighbor, we check the given position has enough adjacent empty spaces in the direction.
    /// </summary>
    public bool IsValid(int xCoord, int yCoord, RandomBoard board)
    {
        // Randomly choose the order to test in case multiple are valid
        PossibleDirections.Shuffle();
        
        foreach (var direction in PossibleDirections)
        {
            CoordinateList.Clear();
            
            (int checkX, int checkY) = (xCoord, yCoord);
            for (int i = 0; i < numberOfValidSpaces; i++)
            {
                (checkX, checkY) = direction.GetCompassTilePos(checkX, checkY);
                if (board.PeekUnoccupiedSpace(checkX, checkY))
                {
                    CoordinateList.Add((checkX, checkY));
                }
                else
                {
                    CoordinateList.Clear();
                    break;
                }
            }

            if (CoordinateList.Count == numberOfValidSpaces)
            {
                return true;
            }
            
        }

        return false;
    }
}
