using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : SingletonMonoBehaviour<EnemySpawner>
{
    /// <summary>
    /// Normal enemies. No spawn restrictions.
    /// </summary>
    public List<TileObjectSchema> NormalEnemies;

    /// <summary>
    /// These enemies are guarded by an item or another enemy.
    /// Meaning when they spawn, additionally spawn another item or enemy.
    /// </summary>
    public List<TileObjectSchema> GuardedEnemies;

    /// <summary>
    /// These enemies spawn in a pattern.
    /// For example gargoyles spawn next to each other and facing each other.
    /// </summary>
    public List<TileObjectSchema> GroupEnemies;

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
    }

    public TileObjectSchema GetRandomNormalEnemy()
    {
        return NormalEnemies[Random.Range(0, NormalEnemies.Count)];
    }

    public TileObjectSchema GetRandomBoss()
    {
        return Bosses[Random.Range(0, Bosses.Count)];
    }

    public TileObjectSchema GetRandomStartingBoon()
    {
        return StartingBoons[Random.Range(0, StartingBoons.Count)];
    }

}
