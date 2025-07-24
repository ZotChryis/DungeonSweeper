using Screens;
using UnityEngine;

public class SettingsBoolChangedListener : MonoBehaviour
{
    public string SettingKey = "";

    public GameObject Target;
    
    private void Start()
    {
        var settingsScreen = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Settings] as SettingsScreen;
        settingsScreen.OnBoolSettingChanged += OnBoolSettingChanged;
        
        if (Target != null)
        {
            bool value = FBPP.GetBool(SettingKey, false);
            Target.SetActive(value);
        }
    }

    private void OnDestroy()
    {
        var settingsScreen = ServiceLocator.Instance.OverlayScreenManager.Screens[OverlayScreenManager.ScreenType.Settings] as SettingsScreen;
        settingsScreen.OnBoolSettingChanged -= OnBoolSettingChanged;
    }
    
    private void OnBoolSettingChanged(string key)
    {
        if (key != SettingKey)
        {
            return;
        }

        if (Target != null)
        {
            bool value = FBPP.GetBool(SettingKey, false);
            Target.SetActive(value);
        }
    }
}
