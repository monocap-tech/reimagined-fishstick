using UnityEngine;
using UnityEngine.UI;

namespace RelayYard
{
    /// <summary>
    /// Shared audio-settings behavior for Menu and Pause. Music and SFX sliders,
    /// mutes and reset all operate on the persistent AudioDirector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioSettingsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Text musicValueLabel;
        [SerializeField] private Text sfxValueLabel;
        [SerializeField] private Text musicMuteButtonLabel;
        [SerializeField] private Text sfxMuteButtonLabel;

        public void Open()
        {
            if (overlay != null) overlay.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (overlay != null) overlay.SetActive(false);
            PlayerPrefs.Save();
        }

        public void ChangeMusic(float value)
        {
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.SetMusicVolume(value);
            Refresh();
        }

        public void ChangeSfx(float value)
        {
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.SetSfxVolume(value);
            Refresh();
        }

        public void ToggleMusicMute()
        {
            AudioDirector audio = AudioDirector.Instance;
            if (audio != null) audio.SetMusicMuted(!audio.MusicMuted);
            Refresh();
        }

        public void ToggleSfxMute()
        {
            AudioDirector audio = AudioDirector.Instance;
            if (audio != null) audio.SetSfxMuted(!audio.SfxMuted);
            Refresh();
        }

        public void ResetAudio()
        {
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.ResetAudioPreferences();
            Refresh();
        }

        public void PreviewEffect()
        {
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.PlayRelay();
        }

        public void Refresh()
        {
            AudioDirector audio = AudioDirector.Instance;
            if (audio == null) return;
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(audio.MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
            if (musicValueLabel != null)
                musicValueLabel.text = Mathf.RoundToInt(audio.MusicVolume * 100f) + "%";
            if (sfxValueLabel != null)
                sfxValueLabel.text = Mathf.RoundToInt(audio.SfxVolume * 100f) + "%";
            if (musicMuteButtonLabel != null)
                musicMuteButtonLabel.text = audio.MusicMuted ? "UNMUTE" : "MUTE";
            if (sfxMuteButtonLabel != null)
                sfxMuteButtonLabel.text = audio.SfxMuted ? "UNMUTE" : "MUTE";
        }

#if UNITY_EDITOR
        public void EditorConfigure(GameObject panel, Slider music, Slider sfx,
            Text musicValue, Text sfxValue, Text musicMute, Text sfxMute)
        {
            overlay = panel;
            musicSlider = music;
            sfxSlider = sfx;
            musicValueLabel = musicValue;
            sfxValueLabel = sfxValue;
            musicMuteButtonLabel = musicMute;
            sfxMuteButtonLabel = sfxMute;
        }
#endif
    }
}
