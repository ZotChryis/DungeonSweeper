using System.Collections.Generic;
using UnityEngine;

// TODO: Do we even need this class now that we have GridSpawnRequirements and GridSpawnSettings?
// Maybe we move all the Grid generation logic where it chooses a location for spawns here?
public class EnemySpawner : SingletonMonoBehaviour<EnemySpawner>
{
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

    public TileObjectSchema GetRandomBoss()
    {
        return Bosses[Random.Range(0, Bosses.Count)];
    }

    public TileObjectSchema GetRandomStartingBoon()
    {
        return StartingBoons[Random.Range(0, StartingBoons.Count)];
    }

}
