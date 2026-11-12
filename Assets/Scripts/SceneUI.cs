using UnityEngine;
using UnityEngine.SceneManagement;

namespace RelayYard
{
    public sealed class SceneUI : MonoBehaviour
    {
        public enum ScreenMode { Menu, Game, Win }
        [SerializeField] private ScreenMode screenMode = ScreenMode.Menu;
        private RelayController controller;

        private void Start()
        {
            if (screenMode == ScreenMode.Game)
                controller = FindAnyObjectByType<RelayController>();
        }

        private void OnGUI()
        {
            int width = 350;
            float left = Mathf.Max(20f, Screen.width * 0.5f - width * 0.5f);
            GUI.skin.label.fontSize = 18;
            GUI.skin.button.fontSize = 18;

            if (screenMode == ScreenMode.Game)
            {
                string status = controller == null ? "Loading..." :
                    "Relays: " + controller.ActiveCount + " / " + controller.TotalCount +
                    (controller.IsSolved ? " — EXIT OPEN" : " — Activate them all");
                GUI.Box(new Rect(12, 12, 480, 55), status);
                if (GUI.Button(new Rect(Screen.width - 115, 12, 100, 42), "Restart"))
                    SceneManager.LoadScene("Game");
                return;
            }

            float top = Mathf.Max(40f, Screen.height * 0.5f - 140f);
            GUI.Box(new Rect(left - 20, top - 25, width + 40, 260), "");
            GUI.Label(new Rect(left, top, width, 35), "RELAY YARD");
            GUI.Label(new Rect(left, top + 45, width, 28),
                screenMode == ScreenMode.Win ? "All relays restored. You win!" :
                "Collect three relays. Reach the exit.");
            if (screenMode == ScreenMode.Menu)
                GUI.Label(new Rect(left, top + 76, width, 28), "A / D to move. Space to jump.");

            if (GUI.Button(new Rect(left, top + 115, width, 44),
                screenMode == ScreenMode.Menu ? "Start" : "Play again"))
                SceneManager.LoadScene("Game");

            AudioDirector audio = AudioDirector.Instance;
            if (audio != null)
            {
                GUI.Label(new Rect(left, top + 170, width, 25), "Music volume");
                float adjusted = GUI.HorizontalSlider(
                    new Rect(left, top + 205, width - 5, 22), audio.MusicVolume, 0f, 1f);
                if (Mathf.Abs(adjusted - audio.MusicVolume) > 0.001f)
                    audio.SetMusicVolume(adjusted);
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(ScreenMode mode)
        {
            screenMode = mode;
        }
#endif
    }
}
