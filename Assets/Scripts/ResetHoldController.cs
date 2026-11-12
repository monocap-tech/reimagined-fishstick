using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RelayYard
{
    /// <summary>
    /// Continuous R hold required for reset. On early release the ACTUAL
    /// pending reset is canceled immediately; only the displayed ring drains.
    /// Mouse click on the same HUD control resets instantly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResetHoldController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private SceneLoader loader;
        [SerializeField] private Image progressRing;
        [SerializeField, Min(0.2f)] private float holdSeconds = 0.9f;
        [SerializeField, Min(0.05f)] private float drainSeconds = 0.25f;

        private LightsOutController board;
        private InputAction holdAction;
        private float heldSeconds;
        private float displayedProgress;
        private bool triggered;

        private void Awake()
        {
            board = FindAnyObjectByType<LightsOutController>();
            holdAction = controls == null ? null : controls.FindAction("Gameplay/ResetHold", false);
            if (holdAction == null)
                Debug.LogWarning("[Relay Yard] ResetHold action is missing from RelayControls.inputactions.");
            if (progressRing != null)
            {
                progressRing.type = Image.Type.Filled;
                progressRing.fillMethod = Image.FillMethod.Radial360;
                progressRing.fillOrigin = (int)Image.Origin360.Top;
                progressRing.fillClockwise = true;
                progressRing.fillAmount = 0f;
                progressRing.raycastTarget = false;
            }
        }

        private void OnEnable() => holdAction?.Enable();

        private void OnDisable()
        {
            holdAction?.Disable();
            heldSeconds = 0f;
            triggered = false;
            displayedProgress = 0f;
            if (progressRing != null) progressRing.fillAmount = 0f;
        }

        private bool CanRestart()
        {
            return Time.timeScale > 0f &&
                   board != null &&
                   !board.IsSolved;
        }

        private void Update()
        {
            bool isHolding = CanRestart() && holdAction != null && holdAction.IsPressed();
            if (isHolding)
            {
                heldSeconds += Time.unscaledDeltaTime;
                displayedProgress = Mathf.Clamp01(heldSeconds / holdSeconds);
                if (!triggered && heldSeconds >= holdSeconds)
                {
                    triggered = true;
                    if (loader != null) loader.RestartGame();
                }
            }
            else
            {
                // Never bank partial presses. The fade-out is visual only.
                heldSeconds = 0f;
                triggered = false;
                displayedProgress = Mathf.MoveTowards(
                    displayedProgress, 0f, Time.unscaledDeltaTime / drainSeconds);
            }

            if (progressRing != null)
                progressRing.fillAmount = displayedProgress;
        }

        public void ClickRestart()
        {
            if (CanRestart() && loader != null)
                loader.RestartGame();
        }

#if UNITY_EDITOR
        public void EditorConfigure(InputActionAsset input, SceneLoader sceneLoader, Image ring)
        {
            controls = input;
            loader = sceneLoader;
            progressRing = ring;
        }
#endif
    }
}
