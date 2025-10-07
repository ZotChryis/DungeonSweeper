using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildTargetEnableButton : MonoBehaviour
{
    private readonly BuildTarget[] QuitGameBuildTargets = new BuildTarget[] {
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneLinux64,
        };

    private void Start()
    {
        gameObject.SetActive(QuitGameBuildTargets.Contains(EditorUserBuildSettings.activeBuildTarget));
    }
}
