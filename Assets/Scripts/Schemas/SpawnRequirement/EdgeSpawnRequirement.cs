using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/Edge")]
public class EdgeSpawnRequirement : NoCheckChildSpawnRequirement
{
    [Serializable]
    [Flags]
    public enum Edge
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
    }

    /// <summary>
    /// Boolean OR requirements.
    /// Any pass will trigger a success.
    /// </summary>
    [SerializeField]
    private Edge RequiredEdges;

    [SerializeField]
    private bool AllowCorners;

    public bool IsValid(int xCoord, int yCoord, RandomBoard board)
    {
        if (RequiredEdges.HasFlag(Edge.Left) && xCoord == 0)
        {
            return !Negate;
        }

        if (RequiredEdges.HasFlag(Edge.Top) && yCoord == board.height - 1)
        {
            return !Negate;
        }

        if (RequiredEdges.HasFlag(Edge.Right) && xCoord == board.width - 1)
        {
            return !Negate;
        }

        if (RequiredEdges.HasFlag(Edge.Bottom) && yCoord == 0)
        {
            return !Negate;
        }

        return Negate;
    }

    public override (int x, int y) GetRandomCoordinate(RandomBoard board)
    {
        CoordinateList.Clear();
        if (RequiredEdges.HasFlag(Edge.Bottom))
        {
            for (int x = 0; x < board.width; x++)
            {
                CoordinateList.Add((x, 0));
            }
        }
        if (RequiredEdges.HasFlag(Edge.Left))
        {
            for (int y = 1; y < board.height; y++)
            {
                CoordinateList.Add((0, y));
            }
            if (!RequiredEdges.HasFlag(Edge.Bottom))
            {
                CoordinateList.Add((0, 0));
            }
        }
        if (RequiredEdges.HasFlag(Edge.Right))
        {
            for (int y = 1; y < board.height; y++)
            {
                CoordinateList.Add((board.width - 1, y));
            }
            if (!RequiredEdges.HasFlag(Edge.Bottom))
            {
                CoordinateList.Add((board.width - 1, 0));
            }
        }
        if (RequiredEdges.HasFlag(Edge.Top))
        {
            for (int x = 1; x < board.width - 1; x++)
            {
                CoordinateList.Add((x, board.height - 1));
            }
            if (!RequiredEdges.HasFlag(Edge.Left))
            {
                CoordinateList.Add((0, board.height - 1));
            }
            if (!RequiredEdges.HasFlag(Edge.Right))
            {
                CoordinateList.Add((board.width - 1, board.height - 1));
            }
        }
        if (!AllowCorners)
        {
            CoordinateList.Remove((0, 0));
            CoordinateList.Remove((board.width - 1, 0));
            CoordinateList.Remove((0, board.height - 1));
            CoordinateList.Remove((board.width - 1, board.height - 1));
        }
        
        RemoveOccupiedSpaces(board);
        
        return CoordinateList.GetRandomItem();
    }
}
