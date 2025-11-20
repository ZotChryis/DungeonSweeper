
using UnityEngine;

public class ActivateOnFreeTierOnly : MonoBehaviour
{
    private void Start()
    {
        bool shouldEnableThis = ServiceLocator.Instance.IsPaidVersion();
        gameObject.SetActive(!shouldEnableThis);
    }
}

