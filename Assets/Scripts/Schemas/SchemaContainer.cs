using System;
using System.Collections.Generic;

// TODO: Fully deprecate/delete because George doesn't want to do it this way
/// <summary>
/// A temporary solution to how we access all the static data for the game.
/// For now, we can load in everything into memory. We will need a system for loading/off-loading assets
/// once the project gets big. For now, this should be sufficient.
/// This requires everything to live under the Assets/Resources/ directory.
/// </summary>
public class SchemaContainer
{
    private const string c_enemyDirectory = "Data/Enemy";
    private const string c_itemDirectory = "Data/Item";
    private const string c_levelProgressionDirectory = "Data/LevelProgression";
    
    public LevelProgressionSchema LevelProgression;
    
    public void Initialize(Schema.ProductionStatus minimumStatus)
    {
        LevelProgression = Array.FindAll(
            UnityEngine.Resources.LoadAll<LevelProgressionSchema>(c_levelProgressionDirectory),
            v => v.Status >= minimumStatus
        )[0];
    }
}
