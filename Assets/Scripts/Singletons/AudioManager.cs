using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Singletons
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        [SerializeField] private AudioSource MusicSource;
        [SerializeField] private AudioSource[] SfxSources;

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
            if (key == string.Empty)
            {
                Debug.Log("Tried to play sfx with empty key.");
                return;
            }

            if (!SfxClips.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning("Tried to play sfx that didn't exist: " + key);
                return;
            }
            Debug.Log("Try playing sfx with key: " + key);
            
            // TODO: Make a batch/pool system where we can support multiple effects at once
            var source = FindBestSfxSource();
            if (source.isPlaying)
            {
                source.Stop();
            }
        
            source.clip = clip;
            source.Play();
        }

        private void RecalibrateVolumes()
        {
            MusicSource.volume = MusicVolume * MasterVolume;
            foreach (var source in SfxSources)
            {
                source.volume = SfxVolume * MasterVolume;
            }
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

        private AudioSource FindBestSfxSource()
        {
            for (var i = 0; i < SfxSources.Length; i++)
            {
                if (SfxSources[i].isPlaying)
                {
                    continue;
                }
                
                return SfxSources[i];
            }
            
            // If all are playing, then we just use the first one and overwrite it
            return SfxSources[0];
        }
    }
}
