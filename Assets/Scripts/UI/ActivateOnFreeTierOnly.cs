
using UnityEngine;

public class ActivateOnFreeTierOnly : MonoBehaviour
{
    private void Start()
    {
        bool isPaidVersion = ServiceLocator.Instance.IsPaidVersion();
        gameObject.SetActive(!isPaidVersion);
    }
}

