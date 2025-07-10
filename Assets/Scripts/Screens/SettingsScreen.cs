using Gameplay;
using System;
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
        [SerializeField] private Toggle SafeMinesToggle;

        protected override void Awake()
        {
            base.Awake();

            MasterVolume.onValueChanged.AddListener(OnMasterVolumeChanged);
            MusicVolume.onValueChanged.AddListener(OnMusicVolumeChanged);
            SFXVolume.onValueChanged.AddListener(OnSFXVolumeChanged);
            SafeMinesToggle.onValueChanged.AddListener(OnSafeMinesChanged);

            LoadInitialVolumes();

            Reset.OnConfirmed += OnResetConfirmed;
        }

        private void OnResetConfirmed()
        {
            ServiceLocator.Instance.DeleteSaveFile();
        }

        private void LoadInitialVolumes()
        {
            MasterVolume.value = FBPP.GetFloat("MasterVolume", 1.0f);
            MusicVolume.value = FBPP.GetFloat("MusicVolume", 1.0f);
            SFXVolume.value = FBPP.GetFloat("SfxVolume", 1.0f);
            SafeMinesToggle.isOn = FBPP.GetBool(PlayerOptions.IsSafeMinesOn, true);
        }

        private void OnSafeMinesChanged(bool value)
        {
            FBPP.SetBool(PlayerOptions.IsSafeMinesOn, value);
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
