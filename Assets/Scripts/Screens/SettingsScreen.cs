using System;
using Gameplay;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
    public class SettingsScreen : BaseScreen
    {
        [SerializeField] private Slider MasterVolume;
        [SerializeField] private Slider MusicVolume;
        [SerializeField] private Slider SFXVolume;
        [SerializeField] private DSButton Reset;
        [SerializeField] private DSButton ResetTutorial;
        [SerializeField] private Toggle SafeMinesToggle;
        [SerializeField] private Toggle AllowLeftHoldContextMenu;
        [SerializeField] private Toggle CanPickUpItems;
        [SerializeField] private Toggle AllowCheats;

        public Action<string> OnBoolSettingChanged;
        
        protected override void Awake()
        {
            base.Awake();

            MasterVolume.onValueChanged.AddListener(OnMasterVolumeChanged);
            MusicVolume.onValueChanged.AddListener(OnMusicVolumeChanged);
            SFXVolume.onValueChanged.AddListener(OnSFXVolumeChanged);
            SafeMinesToggle.onValueChanged.AddListener(OnSafeMinesChanged);
            AllowLeftHoldContextMenu.onValueChanged.AddListener(OnAllowLeftHoldContextMenuChanged);
            CanPickUpItems.onValueChanged.AddListener(OnCanPickUpItemsChanged);
            AllowCheats.onValueChanged.AddListener(OnAllowCheatsChanged);

            LoadInitialVolumes();

            Reset.OnConfirmed += OnResetConfirmed;
            ResetTutorial.OnConfirmed += OnResetTutorialConfirmed;
        }

        private void OnAllowCheatsChanged(bool value)
        {
            FBPP.SetBool(PlayerOptions.AllowCheats, value);
            OnBoolSettingChanged?.Invoke(PlayerOptions.AllowCheats);
        }

        private void OnCanPickUpItemsChanged(bool value)
        {
            FBPP.SetBool(PlayerOptions.CanPickUpItems, value);
            OnBoolSettingChanged?.Invoke(PlayerOptions.CanPickUpItems);
        }

        private void OnResetConfirmed()
        {
            ServiceLocator.Instance.DeleteSaveFile();
        }
        
        private void OnResetTutorialConfirmed()
        {
            ServiceLocator.Instance.ResetTutorial();
        }

        private void LoadInitialVolumes()
        {
            MasterVolume.value = FBPP.GetFloat("MasterVolume", 1.0f);
            MusicVolume.value = FBPP.GetFloat("MusicVolume", 1.0f);
            SFXVolume.value = FBPP.GetFloat("SfxVolume", 1.0f);
            SafeMinesToggle.isOn = FBPP.GetBool(PlayerOptions.IsSafeMinesOn, true);
            CanPickUpItems.isOn = FBPP.GetBool(PlayerOptions.CanPickUpItems, true);
            AllowCheats.isOn = FBPP.GetBool(PlayerOptions.AllowCheats, false);
            AllowLeftHoldContextMenu.isOn = FBPP.GetBool(PlayerOptions.AllowLeftHoldContextMenu, true);
        }

        private void OnSafeMinesChanged(bool value)
        {
            FBPP.SetBool(PlayerOptions.IsSafeMinesOn, value);
            OnBoolSettingChanged?.Invoke(PlayerOptions.IsSafeMinesOn);
        }

        private void OnAllowLeftHoldContextMenuChanged(bool value)
        {
            FBPP.SetBool(PlayerOptions.AllowLeftHoldContextMenu, value);
            OnBoolSettingChanged?.Invoke(PlayerOptions.AllowLeftHoldContextMenu);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            ServiceLocator.Instance.AudioManager.SetMasterVolume(value);
            FBPP.SetFloat("MasterVolume", value);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            ServiceLocator.Instance.AudioManager.SetMusicVolume(value);
            FBPP.SetFloat("MusicVolume", value);
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            ServiceLocator.Instance.AudioManager.SetSfxVolume(value);
            FBPP.SetFloat("SfxVolume", value);
        }
    }
}
