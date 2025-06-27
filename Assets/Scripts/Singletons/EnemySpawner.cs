using Sirenix.OdinInspector;
using System.Collections.Generic;
using Schemas;

public class EnemySpawner : SerializedMonoBehaviour
{
    /// <summary>
    /// These items spawn revealed.
    /// </summary>
    public List<TileSchema> StartingBoons;

    private void Start()
    {
        ServiceLocator.Instance.Register(this);
    }

    public TileSchema GetRandomStartingBoon()
    {
        return StartingBoons[UnityEngine.Random.Range(0, StartingBoons.Count)];
    }

}
