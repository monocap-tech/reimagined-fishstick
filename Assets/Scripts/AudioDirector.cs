using System;
using UnityEngine;
using UnityEngine.Audio;

namespace RelayYard
{
    [DisallowMultipleComponent]
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }
        private const string VolumeKey = "RelayYard.MusicVolume";
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioClip jumpClip;
        private AudioClip relayClip;
        private AudioClip unlockClip;

        public float MusicVolume => musicSource == null ? 0.6f : musicSource.volume;
        public bool MixerConnected { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            sfxSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0;
            sfxSource.spatialBlend = 0;

            AudioMixer mixer = Resources.Load<AudioMixer>("RelayMixer");
            if (mixer != null)
            {
                AudioMixerGroup[] musicGroups = mixer.FindMatchingGroups("Music");
                AudioMixerGroup[] sfxGroups = mixer.FindMatchingGroups("SFX");
                if (musicGroups.Length > 0 && sfxGroups.Length > 0)
                {
                    musicSource.outputAudioMixerGroup = musicGroups[0];
                    sfxSource.outputAudioMixerGroup = sfxGroups[0];
                    MixerConnected = true;
                }
            }

            // Original, synthesized sounds; no third-party audio assets.
            musicSource.clip = CreateMusic();
            jumpClip = MakeTone("Jump", 0.18f, 420f, 720f);
            relayClip = MakeTone("Relay", 0.28f, 600f, 980f);
            unlockClip = MakeTone("Unlock", 0.6f, 310f, 720f);

            SetMusicVolume(PlayerPrefs.GetFloat(VolumeKey, 0.55f));
            musicSource.Play();
        }

        public void SetMusicVolume(float value)
        {
            if (musicSource == null) return;
            musicSource.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, musicSource.volume);
            PlayerPrefs.Save();
        }

        public void PlayJump() => Play(jumpClip, 0.36f);
        public void PlayRelay() => Play(relayClip, 0.45f);
        public void PlayUnlock() => Play(unlockClip, 0.6f);

        private void Play(AudioClip clip, float volume)
        {
            if (sfxSource != null && clip != null)
                sfxSource.PlayOneShot(clip, volume);
        }

        private static AudioClip MakeTone(string label, float seconds, float startHz, float endHz)
        {
            const int rate = 22050;
            int sampleCount = Mathf.RoundToInt(seconds * rate);
            var samples = new float[sampleCount];
            double phase = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                float p = (float)i / sampleCount;
                float frequency = Mathf.Lerp(startHz, endHz, p);
                phase += 2.0 * Math.PI * frequency / rate;
                float envelope = Mathf.Sin(Mathf.PI * p);
                samples[i] = Mathf.Sin((float)phase) * envelope * 0.28f;
            }

            AudioClip clip = AudioClip.Create(label, sampleCount, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateMusic()
        {
            const int rate = 22050;
            const int seconds = 8;
            float[] notes = { 220f, 261.63f, 329.63f, 392f, 293.66f, 261.63f, 246.94f, 196f };
            var samples = new float[rate * seconds];
            double phase = 0;

            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                int noteIndex = Mathf.FloorToInt(t * 2f) % notes.Length;
                float n = notes[noteIndex];
                phase += 2.0 * Math.PI * n / rate;
                float pulse = 0.75f + 0.25f * Mathf.Cos(t * 2f * Mathf.PI);
                samples[i] = (Mathf.Sin((float)phase) +
                              0.2f * Mathf.Sin((float)phase * 2f)) * 0.085f * pulse;
            }

            AudioClip clip = AudioClip.Create("Relay Yard original loop", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
