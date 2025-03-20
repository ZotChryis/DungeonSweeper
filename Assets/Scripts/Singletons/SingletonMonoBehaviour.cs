using UnityEngine;

/// <summary>
/// Crude implementation of the Singleton pattern for MonoBehaviors.
/// Place one instance in your scene.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T: SingletonMonoBehaviour<T>
{
    public static T Instance { get; protected set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = (T)this;
    }
}
