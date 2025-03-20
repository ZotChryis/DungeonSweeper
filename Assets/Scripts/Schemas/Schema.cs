using UnityEngine;

/// <summary>
/// Base class for all static data.
/// </summary>
public class Schema : ScriptableObject
{
    public enum ProductionStatus
    {
        Debug,
        InDevelopment,
        Shippable
    }

    public ProductionStatus Status;
}