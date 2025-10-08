using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildTargetEnableButton : MonoBehaviour
{
    // Platforms this object will not be enabled.
    private readonly RuntimePlatform[] QuitGameBuildTargets = new RuntimePlatform[] {
            RuntimePlatform.Android,
            RuntimePlatform.WebGLPlayer,
        };

    private void Start()
    {
        gameObject.SetActive(!QuitGameBuildTargets.Contains(Application.platform));
    }
}
