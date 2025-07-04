using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Singletons
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        [SerializeField] private AudioSource MusicSource;
        [SerializeField] private AudioSource SfxSource;

        [SerializeField] private SerializedDictionary<string, AudioClip> MusicClips;
        [SerializeField] private SerializedDictionary<string, AudioClip> SfxClips;

        // TODO: Save settings
        private float MasterVolume = 1.0f;
        private float MusicVolume = 1.0f;
        private float SfxVolume = 1.0f;
    
        protected override void Awake()
        {
            base.Awake();
            
            PlayRandomMusic();

            ServiceLocator.Instance.Register(this);
        }

        public void PlayMusic(string key)
        {
            if (!MusicClips.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning("Tried to play music that didn't exist: " + key);
                return;
            }
        
            MusicSource.clip = clip;
            MusicSource.Play();
        }
    
        public void PlaySfx(string key)
        {
            if (!SfxClips.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning("Tried to play sfx that didn't exist: " + key);
                return;
            }
        
            SfxSource.clip = clip;
            SfxSource.Play();
        }

        private void RecalibrateVolumes()
        {
            MusicSource.volume = MusicVolume * MasterVolume;
            SfxSource.volume = SfxVolume * MasterVolume;
        }

        public void PlayRandomMusic()
        {
            string key = MusicClips.Keys.ToList().GetRandomItem();
            PlayMusic(key);
        }
    
        public void SetMasterVolume(float value)
        {
            MasterVolume = value;
            RecalibrateVolumes();
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = value;
            RecalibrateVolumes();
        }
    
        public void SetSfxVolume(float value)
        {
            SfxVolume = value;
            RecalibrateVolumes();
        }
    }
}
