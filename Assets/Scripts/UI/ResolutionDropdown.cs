using System;
using TMPro;
using UnityEngine;

public class ResolutionDropdown : MonoBehaviour
{
    private TMP_Dropdown resolutionDropdown;
    private Resolution nativeResolution;

    private void Start()
    {
#if !UNITY_STANDALONE
        // Only show resolution dropdown for standalone platform (Mac, Windows or Linux).
        gameObject.SetActive(false);
#else
        nativeResolution = Screen.currentResolution;

        resolutionDropdown = GetComponent<TMP_Dropdown>();

        var savedResolutionMode = GetSavedResolutionMode();
        int resolutionModeInt = (int)savedResolutionMode;
        if (resolutionModeInt >= 2)
        {
            resolutionModeInt = 2;
        }
        resolutionDropdown.SetValueWithoutNotify(resolutionModeInt);
        resolutionDropdown.RefreshShownValue();
#endif
    }

    public void SetResolution(int resolution)
    {
        FullScreenMode newMode;
        if (resolution >= 2)
        {
            newMode = FullScreenMode.Windowed;
        }
        else
        {
            newMode = (FullScreenMode)resolution;
        }
        Screen.SetResolution(nativeResolution.width, nativeResolution.height, newMode);
        FBPP.SetString("ResolutionOption", newMode.ToString());
    }

    private FullScreenMode GetSavedResolutionMode()
    {
        string defaultResolutionMode = FullScreenMode.FullScreenWindow.ToString();
        string savedResolutionMode = FBPP.GetString("ResolutionOption", defaultResolutionMode);
        if (!Enum.TryParse(savedResolutionMode, out FullScreenMode savedResolutionModeEnum))
        {
            savedResolutionModeEnum = FullScreenMode.Windowed;
        }
        return savedResolutionModeEnum;
    }
}