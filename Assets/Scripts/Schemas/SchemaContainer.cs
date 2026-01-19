using System.Collections.Generic;
using System.Linq;

// TODO: Fully deprecate/delete because George doesn't want to do it this way
namespace Schemas
{
    /// <summary>
    /// A temporary solution to how we access all the static data for the game.
    /// For now, we can load in everything into memory. We will need a system for loading/off-loading assets
    /// once the project gets big. For now, this should be sufficient.
    /// This requires everything to live under the Assets/Resources/ directory.
    /// </summary>
    public class SchemaContainer
    {
        public static string c_achievement = "Data/Achievement";
        public static string c_challenges = "Data/Challenges";
        public static string c_tileObject = "Data/TileObject";
        public static string c_item = "Data/Item";
        public static string c_class = "Data/Class";
        public static string c_levelProgressionDirectory = "Data/LevelProgression";

        public List<TileSchema> TileObjectSchemas;
        public List<ItemSchema> ItemSchemas;
        public List<ClassSchema> ClassSchemas;
        public List<AchievementSchema> AchievementSchemas;
        public List<ChallengeSchema> ChallengeSchemas;
    
        public LevelProgressionSchema LevelProgression;

        public void Initialize()
        {
            TileObjectSchemas = UnityEngine.Resources.LoadAll<TileSchema>(c_tileObject).ToList();
            ItemSchemas = UnityEngine.Resources.LoadAll<ItemSchema>(c_item).ToList();
            ClassSchemas = UnityEngine.Resources.LoadAll<ClassSchema>(c_class).ToList();
            LevelProgression = UnityEngine.Resources.LoadAll<LevelProgressionSchema>(c_levelProgressionDirectory)[0];
            AchievementSchemas = UnityEngine.Resources.LoadAll<AchievementSchema>(c_achievement).ToList();
            ChallengeSchemas = UnityEngine.Resources.LoadAll<ChallengeSchema>(c_challenges).ToList();
        }
    }
}
