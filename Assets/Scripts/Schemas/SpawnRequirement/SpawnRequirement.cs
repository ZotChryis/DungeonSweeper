using System;
using UnityEngine;

public abstract class SpawnRequirement : Schema
{
    [Serializable]
    public struct RequirementOption
    {
        public bool UseRequirement;
        public bool Value;
    }
    
    [SerializeField] protected bool Negate;

    public abstract bool IsValid(int xCoord, int yCoord);
}