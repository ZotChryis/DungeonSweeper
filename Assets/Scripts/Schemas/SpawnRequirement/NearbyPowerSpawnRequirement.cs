using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpawnRequirement/NearbyPowerSpawnRequirement")]
public class NearbyPowerSpawnRequirement : SpawnRequirement
{
    /// <summary>
    /// The power of the object to search for.
    /// </summary>
    [SerializeField]
    private int PowerOfObject;

    /// <summary>
    /// The max distance away the tile object needs to exist in. This is a Manhattan distance.
    /// </summary>
    [SerializeField]
    private int Distance;

    /// <summary>
    /// Allow to dictate whether you share the same row or not.
    /// </summary>
    [SerializeField]
    private RequirementOption SameRow;

    /// <summary>
    /// Allow to dictate whether you share the same column or not.
    /// </summary>
    [SerializeField]
    private RequirementOption SameColumn;

    /// <summary>
    /// Allows to dictate wether you share the same row OR column.
    /// Using the two above together is AND.
    /// </summary>
    [SerializeField]
    private RequirementOption SameRowOrColumn;

    /// <summary>
    /// Allow to dictate row position relative to found object.
    /// </summary>
    [SerializeField]
    private RequirementOption HigherRow;

    /// <summary>
    /// Allow to dictate column position relative to found object.
    /// </summary>
    [SerializeField]
    private RequirementOption HigherColumn;

    /// <summary>
    /// Whether or not to add this spawn to the found object's Tracked list.
    /// </summary>
    [SerializeField]
    private bool Track;

    public override bool IsValid(int xCoord, int yCoord)
    {
        Grid grid = ServiceLocator.Instance.Grid;

        for (int i = -Distance; i <= Distance; i++)
        {
            for (int j = -Distance; j <= Distance; j++)
            {
                if (!grid.InGridBounds(xCoord + i, yCoord + j))
                {
                    continue;
                }

                // Don't check yourself.
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (SameRow.UseRequirement && (SameRow.Value ? j != 0 : j == 0))
                {
                    continue;
                }

                if (SameColumn.UseRequirement && (SameColumn.Value ? i != 0 : i == 0))
                {
                    continue;
                }

                if (SameRowOrColumn.UseRequirement)
                {
                    if (SameRowOrColumn.Value && (i != 0 && j != 0))
                    {
                        continue;
                    }

                    if (!SameRowOrColumn.Value && (i == 0 || j == 0))
                    {
                        continue;
                    }
                }

                if (grid.GetObject(xCoord + i, yCoord + j)?.Power == PowerOfObject)
                {
                    if (HigherRow.UseRequirement && (HigherRow.Value ? i < 0 : i > 0))
                    {
                        continue;
                    }

                    if (HigherColumn.UseRequirement && (HigherColumn.Value ? j < 0 : j > 0))
                    {
                        continue;
                    }

                    return !Negate;
                }
            }
        }

        return Negate;
    }
}
