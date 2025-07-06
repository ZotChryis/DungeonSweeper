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

        protected override void Awake()
        {
            base.Awake();

            MasterVolume.onValueChanged.AddListener(OnMasterVolumeChanged);
            MusicVolume.onValueChanged.AddListener(OnMusicVolumeChanged);
            SFXVolume.onValueChanged.AddListener(OnSFXVolumeChanged);

            LoadInitialVolumes();

            Reset.OnConfirmed += OnResetConfirmed;
        }

        private void OnResetConfirmed()
        {
            ServiceLocator.Instance.DeleteSaveFile();
        }

        private void LoadInitialVolumes()
        {
            float masterVolume = FBPP.GetFloat("MasterVolume", 1.0f);
            float musicVolume = FBPP.GetFloat("MusicVolume", 1.0f);
            float sfxVolume = FBPP.GetFloat("SfxVolume", 1.0f);
 
            MasterVolume.value = masterVolume;
            MusicVolume.value = musicVolume;
            SFXVolume.value = sfxVolume;
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
