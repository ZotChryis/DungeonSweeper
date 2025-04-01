using Sirenix.OdinInspector;
using System.Collections.Generic;

public class EnemySpawner : SerializedMonoBehaviour
{
    /// <summary>
    /// Normal enemies. No spawn restrictions. Each enemy in this list must be unique in it's power.
    /// TODO: This is probably not needed delete me.
    /// </summary>
    public List<TileObjectSchema> NormalEnemies;

    /// <summary>
    /// Normal enemies. No spawn restrictions.
    /// Value is number of enemies to spawn.
    /// </summary>
    public Dictionary<TileObjectSchema, int> NormalEnemyToSpawnCount;

    /// <summary>
    /// Shows how many normal enemies will spawn.
    /// </summary>
    [ShowInInspector]
    private int MaxNumberOfNormalEnemies;

    /// <summary>
    /// These enemies are guarded by an item or another enemy.
    /// Meaning when they spawn, additionally spawn another item or enemy.
    /// They spawn in order.
    /// DO NOT REORDER THIS LIST
    /// </summary>
    public List<TileObjectSchema> GuardedEnemies;

    /// <summary>
    /// Corresponding guarded enemy spawn count.
    /// DO NOT REORDER THIS LIST
    /// </summary>
    public List<int> GuardedEnemySpawnCount;

    /// <summary>
    /// These enemies spawn in a pattern.
    /// For example gargoyles spawn next to each other and facing each other.
    /// They spawn in order.
    /// DO NOT REORDER THIS LIST
    /// </summary>
    public List<TileObjectSchema> GroupEnemies;

    /// <summary>
    /// Corresponding group enemy spawn count.
    /// DO NOT REORDER THIS LIST
    /// </summary>
    public List<int> GroupEnemySpawnCount;

    /// <summary>
    /// These enemies spawn revealed.
    /// </summary>
    public List<TileObjectSchema> Bosses;

    /// <summary>
    /// These items spawn revealed.
    /// </summary>
    public List<TileObjectSchema> StartingBoons;

    private void Start()
    {
        ServiceLocator.Instance.Register(this);
        MaxNumberOfNormalEnemies = 0;
        foreach(var numberOfEnemies in NormalEnemyToSpawnCount.Values)
        {
            MaxNumberOfNormalEnemies += numberOfEnemies;
        }
    }

    public TileObjectSchema GetRandomBoss()
    {
        return Bosses[UnityEngine.Random.Range(0, Bosses.Count)];
    }

    public TileObjectSchema GetRandomStartingBoon()
    {
        return StartingBoons[UnityEngine.Random.Range(0, StartingBoons.Count)];
    }

}
