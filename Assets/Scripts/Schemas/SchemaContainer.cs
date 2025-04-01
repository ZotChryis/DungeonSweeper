using System;
using System.Collections.Generic;
using System.Linq;

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

    public List<TileObjectSchema> TileObjectSchemas;
    public LevelProgressionSchema LevelProgression;
    
    public void Initialize(Schema.ProductionStatus minimumStatus)
    {
        var enemies = Array.FindAll(
            UnityEngine.Resources.LoadAll<TileObjectSchema>(c_enemyDirectory),
            v => v.Status >= minimumStatus
        ).ToList();
        var items = Array.FindAll(
            UnityEngine.Resources.LoadAll<TileObjectSchema>(c_itemDirectory),
            v => v.Status >= minimumStatus
        ).ToList();

        TileObjectSchemas = new List<TileObjectSchema>();
        TileObjectSchemas.AddRange(enemies);
        TileObjectSchemas.AddRange(items);
        
        LevelProgression = Array.FindAll(
            UnityEngine.Resources.LoadAll<LevelProgressionSchema>(c_levelProgressionDirectory),
            v => v.Status >= minimumStatus
        )[0];
    }
}
