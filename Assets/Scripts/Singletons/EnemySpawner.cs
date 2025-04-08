using Sirenix.OdinInspector;
using System.Collections.Generic;

public class EnemySpawner : SerializedMonoBehaviour
{
    /// <summary>
    /// These items spawn revealed.
    /// </summary>
    public List<TileObjectSchema> StartingBoons;

    private void Start()
    {
        ServiceLocator.Instance.Register(this);
    }

    public TileObjectSchema GetRandomStartingBoon()
    {
        return StartingBoons[UnityEngine.Random.Range(0, StartingBoons.Count)];
    }

}
