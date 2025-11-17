using UnityEngine;

public class ActivateOnStandaloneOnly : MonoBehaviour
{
    private void Start()
    {
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX || UNITY_IOS || UNITY_ANDROID)
        // Only show this gameobject for standalone platform (Mac, Windows or Linux) or Android or IOS.
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif
    }
}
