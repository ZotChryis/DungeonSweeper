using System;
using System.Collections.Generic;

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
    
    public IReadOnlyList<EnemySchema> Enemies;
    public IReadOnlyList<ItemSchema> Items;
    public LevelProgressionSchema LevelProgression;
    
    public void Initialize(Schema.ProductionStatus minimumStatus)
    {
        Enemies = Array.FindAll(
            UnityEngine.Resources.LoadAll<EnemySchema>(c_enemyDirectory),
            v => v.Status >= minimumStatus
        );
        
        Items = Array.FindAll(
            UnityEngine.Resources.LoadAll<ItemSchema>(c_itemDirectory),
            v => v.Status >= minimumStatus
        );

        LevelProgression = Array.FindAll(
            UnityEngine.Resources.LoadAll<LevelProgressionSchema>(c_levelProgressionDirectory),
            v => v.Status >= minimumStatus
        )[0];
    }

    /// <summary>
    /// Forcibly finds the anything but the Dragon.
    /// Super temp function, replace eventually...
    /// </summary>
    /// <returns></returns>
    public EnemySchema TEMP_GetNonDragon()
    {
        while (true)
        {
            EnemySchema enemy = Enemies[UnityEngine.Random.Range(0, ServiceLocator.Instance.Schemas.Enemies.Count)];
            if (enemy.GetPower() == 13)
            {
                continue;
            }

            return enemy;
        }
    }
    
    /// <summary>
    /// Forcibly finds the Dragon (power 13).
    /// Super temp function, replace eventually...
    /// </summary>
    public EnemySchema TEMP_GetDragon()
    {
        foreach (var enemySchema in Enemies)
        {
            if (enemySchema.GetPower() == 13)
            {
                return enemySchema;
            }
        }

        return null;
    }

    /// <summary>
    /// Forcibly finds the VisionOrb item.
    /// Super temp function, replace eventually...
    /// </summary>
    public ITileObject TEMP_GetVisionOrb()
    {
        // Only 1 item right now, it's the first one
        return Items[0];
    }
}
