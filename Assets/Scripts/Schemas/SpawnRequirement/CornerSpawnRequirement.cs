using System;
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
    
    public override bool IsValid(int xCoord, int yCoord)
    {
        Grid grid = ServiceLocator.Instance.Grid;
        foreach (Corner edge in Requirements)
        {
            if (edge == Corner.BottomLeft && xCoord == 0 && yCoord == 0)
            {
                return true;
            }

            if (edge == Corner.BottomRight && xCoord == grid.GetWidth() - 1 && yCoord == 0)
            {
                return true;
            }

            if (edge == Corner.TopLeft && xCoord == 0 && yCoord == grid.GetHeight() - 1)
            {
                return true;
            }

            if (edge == Corner.TopRight && xCoord == grid.GetWidth() - 1 && yCoord == grid.GetHeight() - 1)
            {
                return true;
            }
        }

        return false;
    }
}
