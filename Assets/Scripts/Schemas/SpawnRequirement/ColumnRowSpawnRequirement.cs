using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/MarginLocation")]
public class ColumnRowSpawnRequirement : SpawnRequirement
{
    public List<int> validXMargins;
    public List<int> validYMargins;

    /// <summary>
    /// Potential spaces that should be adjacent.
    /// </summary>
    public CompassDirections[] EmptyAdjacentSpaces;

    // Column row doesn't support consecutive spawns.
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
        List<int> validXColumns = new List<int>(validXMargins.Count * 2);
        List<int> validYRows = new List<int>(validYMargins.Count * 2);
        foreach (int x in validXMargins)
        {
            validXColumns.Add(x);
            validXColumns.Add(board.width - 1 - x);
        }
        foreach (int y in validYMargins)
        {
            validYRows.Add(y);
            validYRows.Add(board.height - 1 - y);
        }
        validXColumns.Shuffle();
        validYRows.Shuffle();

        foreach (int x in validXColumns)
        {
            foreach (int y in validYRows)
            {
                if (board.PeekUnoccupiedSpace(x, y))
                {
                    return (x, y);
                }
            }
        }

        Debug.Log("Somehow ColumnRowSpawnRequirement failed to get any valid location...");
        return board.PeekUnoccupiedRandomSpace();
    }
}
