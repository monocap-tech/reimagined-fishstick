using UnityEngine;
using UnityEngine.SceneManagement;

namespace RelayYard
{
    // Midterm-style scene switching: UI calls LoadScene(string); goal uses
    // an optional trigger, with a gate guard to prevent premature victory.
    [DisallowMultipleComponent]
    public sealed class SceneLoader : MonoBehaviour
    {
        [SerializeField] private bool loadOnPlayerTrigger;
        [SerializeField] private string triggerScene = "Win";
        [SerializeField] private ExitGate gateRequiredOpen;

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName) ||
                !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[SceneLoader] Scene is missing from the active Build Profiles scene list: " + sceneName);
                return;
            }
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadGame() => LoadScene("Game");
        public void LoadMenu() => LoadScene("MainMenu");
        public void RestartGame() => LoadScene("Game");

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!loadOnPlayerTrigger || other.GetComponent<PlayerMotor>() == null)
                return;
            if (gateRequiredOpen != null && !gateRequiredOpen.IsOpen)
                return;
            LoadScene(triggerScene);
        }

#if UNITY_EDITOR
        public void EditorConfigureTrigger(string sceneName, ExitGate gate)
        {
            loadOnPlayerTrigger = true;
            triggerScene = sceneName;
            gateRequiredOpen = gate;
        }
#endif
    }
}
