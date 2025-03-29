using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : SingletonMonoBehaviour<EnemySpawner>
{
    /// <summary>
    /// Normal enemies. No spawn restrictions.
    /// </summary>
    public List<EnemySchema> NormalEnemies;

    /// <summary>
    /// These enemies are guarded by an item or another enemy.
    /// Meaning when they spawn, additionally spawn another item or enemy.
    /// </summary>
    public List<EnemySchema> GuardedEnemies;

    /// <summary>
    /// These enemies spawn in a pattern.
    /// For example gargoyles spawn next to each other and facing each other.
    /// </summary>
    public List<EnemySchema> GroupEnemies;

    /// <summary>
    /// These enemies spawn revealed.
    /// </summary>
    public List<EnemySchema> Bosses;

    /// <summary>
    /// These items spawn revealed.
    /// </summary>
    public List<ItemSchema> StartingBoons;

    private void Start()
    {
        ServiceLocator.Instance.EnemySpawner = this;
    }

    public EnemySchema GetRandomNormalEnemy()
    {
        return NormalEnemies[Random.Range(0, NormalEnemies.Count)];
    }

    public EnemySchema GetRandomBoss()
    {
        return Bosses[Random.Range(0, Bosses.Count)];
    }

    public ITileObject GetRandomStartingBoon()
    {
        return StartingBoons[Random.Range(0, StartingBoons.Count)];
    }

}
