using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NeonRange
{
    /// <summary>Canvas pause controls, asset-backed Escape input, and safe scene exit.</summary>
    public sealed class PauseManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference pauseAction;
        [SerializeField] private GameObject pauseMenuUI;
        private bool isPaused;
        private void Awake() => Resume();
        private void OnEnable() => pauseAction.action.Enable();
        private void Update()
        {
            if (pauseAction.action.WasPressedThisFrame())
            {
                if (isPaused) Resume(); else Pause();
            }
        }
        public void Pause()
        {
            isPaused = true;
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
        }
        public void Resume()
        {
            isPaused = false;
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
        }
        public void QuitToMenu()
        {
            PlayerPrefs.Save();
            Resume();
            SceneManager.LoadScene("MainMenu");
        }
        private void OnDisable()
        {
            pauseAction.action.Disable();
            Time.timeScale = 1f;
        }
        private void OnDestroy() => Time.timeScale = 1f;
    }
}
