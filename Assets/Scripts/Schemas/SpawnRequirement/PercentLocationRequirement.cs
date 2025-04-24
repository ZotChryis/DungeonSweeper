using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/Percent")]
public class PercentLocationRequirement : SpawnRequirement
{
    [SerializeField]
    private CompassDirections[] PotentialAdjacentLocations;

    public float XPercent = 0.5f;

    public float YPercent = 0.5f;

    public override List<(int x, int y)> GetRandomConsecutiveNeighborLocations(RandomBoard board, int inputX, int inputY)
    {
        CoordinateList.Clear();
        foreach (var addLocation in PotentialAdjacentLocations)
        {
            (int, int) adjacent = addLocation.GetCompassTilePos(inputX, inputY);
            if(board.PeekUnoccupiedSpace(adjacent.Item1, adjacent.Item2))
            {
                CoordinateList.Add(adjacent);
            }
        }
        return CoordinateList;
    }

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        return ((int)(board.width * XPercent), (int)(board.height * YPercent));
    }
}
