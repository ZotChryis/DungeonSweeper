using System.Collections.Generic;
using System.Linq;
using Schemas;

// TODO: Fully deprecate/delete because George doesn't want to do it this way
/// <summary>
/// A temporary solution to how we access all the static data for the game.
/// For now, we can load in everything into memory. We will need a system for loading/off-loading assets
/// once the project gets big. For now, this should be sufficient.
/// This requires everything to live under the Assets/Resources/ directory.
/// </summary>
public class SchemaContainer
{
    private const string c_tileObjects = "Data/TileObject";
    private const string c_items = "Data/Items";
    private const string c_levelProgressionDirectory = "Data/LevelProgression";

    public List<TileObjectSchema> TileObjectSchemas;
    public List<ItemSchema> ItemSchemas;
    
    public LevelProgressionSchema LevelProgression;
    
    public void Initialize()
    {
        TileObjectSchemas = UnityEngine.Resources.LoadAll<TileObjectSchema>(c_tileObjects).ToList();
        ItemSchemas = UnityEngine.Resources.LoadAll<ItemSchema>(c_items).ToList();
        LevelProgression = UnityEngine.Resources.LoadAll<LevelProgressionSchema>(c_levelProgressionDirectory)[0];
    }
}
