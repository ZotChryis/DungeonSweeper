using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/Edge")]
public class EdgeSpawnRequirement : SpawnRequirement
{
    [Serializable]
    public enum Edge
    {
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// Boolean OR requirements.
    /// Any pass will trigger a success.
    /// </summary>
    [SerializeField] 
    private Edge[] Requirements;
    
    public override bool IsValid(int xCoord, int yCoord)
    {
        Grid grid = ServiceLocator.Instance.Grid;
        foreach (Edge edge in Requirements)
        {
            if (edge == Edge.Left && xCoord == 0)
            {
                return true;
            }

            if (edge == Edge.Top && yCoord == grid.GetHeight() - 1)
            {
                return true;
            }

            if (edge == Edge.Right && xCoord == grid.GetWidth() - 1)
            {
                return true;
            }

            if (edge == Edge.Bottom && yCoord == 0)
            {
                return true;
            }
        }

        return false;
    }
}
