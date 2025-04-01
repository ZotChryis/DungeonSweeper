using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GridSpawnRequirement")]
public class GridSpawnRequirement : Schema
{
    [Serializable]
    public struct PairedObject
    {
        public int XCoordinateOffset;
        public int YCoordinateOffset;
        public TileObjectSchema ObjectSchema;
    }
    
    /// <summary>
    /// The spawn location must have one of these X Coordinates.
    /// </summary>
    public int[] RequiredXCoordinates;
    
    /// <summary>
    /// The spawn location must have one of these Y Coordinates.
    /// </summary>
    public int[] RequiredYCoordinates;
    
    /// <summary>
    /// These objects must be paired at these specific offsets.
    /// </summary>
    public PairedObject[] ObjectRequirements;

    public bool IsValid(int xCoordinate, int yCoordinate)
    {
        var grid = ServiceLocator.Instance.Grid;
        if (!grid.InGridBounds(xCoordinate, yCoordinate))
        {
            return false;
        }
        
        if (RequiredXCoordinates != null && RequiredXCoordinates.Length > 0 && !RequiredXCoordinates.Contains(xCoordinate))
        {
            return false;
        }
        
        if (RequiredYCoordinates != null && RequiredYCoordinates.Length > 0 && !RequiredYCoordinates.Contains(yCoordinate))
        {
            return false;
        }

        foreach (var objectRequirement in ObjectRequirements)
        {
            if (!grid.InGridBounds(xCoordinate + objectRequirement.XCoordinateOffset, yCoordinate + objectRequirement.YCoordinateOffset))
            {
                return false;
            }
            
            TileObjectSchema tileObject = ServiceLocator.Instance.Grid.GetObject(
                xCoordinate + objectRequirement.XCoordinateOffset, yCoordinate + objectRequirement.YCoordinateOffset);
            if (tileObject != null)
            {
                return false;
            }
        }

        return true;
    }
}