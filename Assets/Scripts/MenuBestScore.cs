
using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeonRange
{
    public sealed class MenuBestScore : MonoBehaviour
    {
        [Header("Main Menu UI")]
        [SerializeField] private TMP_Text startButtonText;
        [SerializeField] private GameObject oldBestScoreObject;

        private void OnEnable()
        {
            RefreshMenu();
        }

        private void Update()
        {
            if (ResetCombinationPressed())
            {
                ResetBestScore();
            }
        }

        private bool ResetCombinationPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            bool ctrl = keyboard.leftCtrlKey.isPressed ||
                        keyboard.rightCtrlKey.isPressed;

            bool shift = keyboard.leftShiftKey.isPressed ||
                         keyboard.rightShiftKey.isPressed;

            return ctrl && shift && keyboard.rKey.wasPressedThisFrame;

#elif ENABLE_LEGACY_INPUT_MANAGER
            bool ctrl = Input.GetKey(KeyCode.LeftControl) ||
                        Input.GetKey(KeyCode.RightControl);

            bool shift = Input.GetKey(KeyCode.LeftShift) ||
                         Input.GetKey(KeyCode.RightShift);

            return ctrl && shift && Input.GetKeyDown(KeyCode.R);
#else
            return false;
#endif
        }

        public void RefreshMenu()
        {
            int best = PlayerPrefs.GetInt(
                ScoreManager.BestScoreKey, 0);

            if (startButtonText != null)
            {
                startButtonText.text =
                    best > 0 ? $"Best: {best}" : "Start";
            }

            // Hide the old standalone score display.
            if (oldBestScoreObject != null)
            {
                oldBestScoreObject.SetActive(false);
            }
        }

        public void ResetBestScore()
        {
            PlayerPrefs.DeleteKey(ScoreManager.BestScoreKey);
            PlayerPrefs.Save();

            RefreshMenu();

            Debug.Log("Best score reset.");
        }
    }
}
