using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quadrant refers to the 4 infinite regions bounded by the x and y axis.
/// Not allowed to spawn on the actual axis.
/// </summary>
[CreateAssetMenu(menuName = "Data/SpawnRequirement/Quadrant")]
public class Quadrant : SpawnRequirement
{
    public CompassDirections SpawnArea;
    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        return new List<(int x, int y)>(0);
    }

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        // centerX and  Y represent the dragon's position
        int centerX = (int)(board.width / 2f); // level 1 = 6
        int centerY = (int)(board.height / 2f); // level 1 = 5
        int xPos = 0, yPos = 0;
        int retX = 0, retY = 0;
        // calculate board quadrant possible locations.
        if (SpawnArea == CompassDirections.NorthEast)
        {
            xPos = Random.Range(centerX + 1, board.width);
            yPos = Random.Range(centerY + 1, board.height);
            (retX, retY) = board.GetNextUnoccupiedSpaceWithMargin(centerX + 1, 0, 0, centerY + 1, xPos, yPos);
        }
        if (SpawnArea == CompassDirections.NorthWest)
        {
            xPos = Random.Range(0, centerX);
            yPos = Random.Range(centerY + 1, board.height);
            (retX, retY) = board.GetNextUnoccupiedSpaceWithMargin(0, centerX + 1, 0, centerY + 1, xPos, yPos);
        }
        if (SpawnArea == CompassDirections.SouthEast)
        {
            xPos = Random.Range(centerX + 1, board.width);
            yPos = Random.Range(0, centerY + 1);
            (retX, retY) = board.GetNextUnoccupiedSpaceWithMargin(centerX + 1, 0, centerY + 1, 0, xPos, yPos);
        }
        if(SpawnArea == CompassDirections.SouthWest)
        {
            xPos = Random.Range(0, centerX + 1);
            yPos = Random.Range(0, centerY + 1);
            (retX, retY) = board.GetNextUnoccupiedSpaceWithMargin(0, centerX + 1, centerY + 1, 0, xPos, yPos);
        }
        Debug.Log("Quadrant would return " + xPos + ", " + yPos + ", but instead: " + retX + ", " + retY);
        return (retX, retY);
    }
}
