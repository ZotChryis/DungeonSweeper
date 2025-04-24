public enum CompassDirections
{
    None,
    North,
    East,
    South,
    West,
    NorthEast,
    SouthEast,
    SouthWest,
    NorthWest,
}

public static class CompassDirectionsExtensions
{
    public static (int x, int y) GetCompassTilePos(this CompassDirections direction, int x, int y)
    {
        switch (direction)
        {
            case CompassDirections.North:
                return (x, y + 1);
            case CompassDirections.East:
                return (x + 1, y);
            case CompassDirections.South:
                return (x, y - 1);
            case CompassDirections.West:
                return (x - 1, y);
            case CompassDirections.NorthEast:
                return (x + 1, y + 1);
            case CompassDirections.SouthEast:
                return (x + 1, y - 1);
            case CompassDirections.SouthWest:
                return (x - 1, y - 1);
            case CompassDirections.NorthWest:
                return (x - 1, y + 1);
            case CompassDirections.None:
            default:
                return (x, y);
        }
    }
}
