using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonRange
{
    /// <summary>Loads Gameplay and provides a desktop-build exit button.</summary>
    public sealed class MainMenu : MonoBehaviour
    {
        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Gameplay");
        }
        public void QuitGame()
        {
            PlayerPrefs.Save();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
