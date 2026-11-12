using UnityEngine;
using UnityEngine.UI;

namespace RelayYard
{
    [RequireComponent(typeof(SceneLoader))]
    public sealed class SceneUI : MonoBehaviour
    {
        public enum ScreenMode { Menu, Game, Win }
        [SerializeField] private ScreenMode screenMode = ScreenMode.Menu;
        private LightsOutController controller;
        private SceneLoader loader;
        [SerializeField] private bool useCanvasUI;
        [SerializeField] private Text progressLabel;
        [SerializeField] private Slider musicVolumeSlider;

        private void Awake() => loader = GetComponent<SceneLoader>();

        private void Start()
        {
            if (screenMode == ScreenMode.Game)
                controller = FindAnyObjectByType<LightsOutController>();
            if (musicVolumeSlider != null && AudioDirector.Instance != null)
                musicVolumeSlider.SetValueWithoutNotify(AudioDirector.Instance.MusicVolume);
        }

        private void Update()
        {
            if (progressLabel == null) return;
            string status = controller == null ? "Loading puzzle..." :
                "LIGHTS " + controller.LitCount + " / " + controller.TotalCount +
                (controller.IsSolved ? "   |   GATE OPEN" : "   |   STEP TO TOGGLE • JUMP TO SKIP");
            if (progressLabel.text != status) progressLabel.text = status;
        }

        public void OnMusicVolumeChanged(float value)
        {
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.SetMusicVolume(value);
        }

        private void OnGUI()
        {
            if (useCanvasUI) return;
            int width = 390;
            float left = Mathf.Max(20f, Screen.width * 0.5f - width * 0.5f);
            GUI.skin.label.fontSize = 18;
            GUI.skin.button.fontSize = 18;

            if (screenMode == ScreenMode.Game)
            {
                string status = controller == null ? "Loading..." :
                    "Lights: " + controller.LitCount + " / " + controller.TotalCount +
                    (controller.IsSolved ? " | GATE OPEN" : " | Step on plates; jump to skip");
                GUI.Box(new Rect(12, 12, Mathf.Min(Screen.width - 135, 495), 54), status);
                if (GUI.Button(new Rect(Screen.width - 115, 12, 103, 42), "Restart"))
                    loader.LoadScene("Game");
                return;
            }

            float top = Mathf.Max(40f, Screen.height * 0.5f - 145f);
            GUI.Box(new Rect(left - 20, top - 25, width + 40, 280), "");
            GUI.Label(new Rect(left, top, width, 35), "RELAY YARD");
            GUI.Label(new Rect(left, top + 43, width, 32),
                screenMode == ScreenMode.Win ? "All six lights on. Gate restored!" :
                "Switches toggle themselves and their neighbors.");
            if (screenMode == ScreenMode.Menu)
                GUI.Label(new Rect(left, top + 78, width, 48),
                    "Step on a plate to toggle lights.\nJump over plates to avoid them.");

            if (GUI.Button(new Rect(left, top + 132, width, 44),
                screenMode == ScreenMode.Menu ? "Start" : "Play again"))
                loader.LoadScene("Game");

            AudioDirector audio = AudioDirector.Instance;
            if (audio != null)
            {
                GUI.Label(new Rect(left, top + 192, width, 25), "Music volume");
                float adjusted = GUI.HorizontalSlider(
                    new Rect(left, top + 228, width - 5, 22), audio.MusicVolume, 0f, 1f);
                if (Mathf.Abs(adjusted - audio.MusicVolume) > 0.001f)
                    audio.SetMusicVolume(adjusted);
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(ScreenMode mode) => screenMode = mode;

        public void EditorUseCanvasUI(Text label, Slider volume)
        {
            progressLabel = label;
            musicVolumeSlider = volume;
            useCanvasUI = true;
        }
#endif
    }
}
