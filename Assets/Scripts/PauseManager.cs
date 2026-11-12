using UnityEngine;
using UnityEngine.InputSystem;

namespace RelayYard
{
    /// <summary>
    /// Assignment 6 / Project 1 pause pattern adapted for Relay Yard.
    /// Escape remains readable during Time.timeScale = 0 because Update continues.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private AudioSettingsPanel audioSettings;

        private InputAction pauseAction;
        private bool isPaused;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (controls != null)
            {
                var gameplay = controls.FindActionMap("Gameplay", false);
                pauseAction = gameplay == null ? null : gameplay.FindAction("Pause", false);
            }
            if (pauseAction == null)
                Debug.LogWarning("[Relay Yard] Pause Input Action missing; check RelayControls.inputactions.");
        }

        private void OnEnable() => pauseAction?.Enable();

        private void Update()
        {
            if (pauseAction != null && pauseAction.WasPressedThisFrame())
                TogglePause();
        }

        public void TogglePause()
        {
            if (isPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            if (isPaused) return;
            isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Resume()
        {
            if (audioSettings != null) audioSettings.Close();
            isPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void RestartPuzzle()
        {
            Resume();
            if (sceneLoader != null) sceneLoader.RestartGame();
        }

        public void QuitToMenu()
        {
            Resume();
            PlayerPrefs.Save();
            if (sceneLoader != null) sceneLoader.LoadMenu();
        }

        private void OnDisable()
        {
            pauseAction?.Disable();
            Time.timeScale = 1f;
            isPaused = false;
        }

        private void OnDestroy() => Time.timeScale = 1f;

#if UNITY_EDITOR
        public void EditorConfigure(InputActionAsset input, GameObject panel, SceneLoader loader,
            AudioSettingsPanel settings = null)
        {
            controls = input;
            pausePanel = panel;
            sceneLoader = loader;
            audioSettings = settings;
        }
#endif
    }
}
