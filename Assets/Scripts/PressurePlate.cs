using UnityEngine;

namespace RelayYard
{
    // Contact is edge-triggered: staying on a plate does not press it again.
    // Re-entering after leaving is a new press, so the puzzle remains reversible.
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PressurePlate : MonoBehaviour
    {
        [SerializeField] private int switchIndex;
        [SerializeField] private LightsOutController puzzle;
        [SerializeField] private SpriteRenderer plateRenderer;
        [SerializeField] private SwitchStyle style;
        [SerializeField] private ParticleSystem sparks;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMotor>() == null || puzzle == null)
                return;
            if (!puzzle.Press(switchIndex))
                return;
            if (sparks != null)
                sparks.Emit(16);
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.PlayRelay();
        }

        public void ShowState(bool lit)
        {
            if (plateRenderer != null && style != null)
                plateRenderer.color = lit ? style.OnColor : style.OffColor;
        }

        public Color CurrentOnColor => style == null ? Color.cyan : style.OnColor;
        public Color CurrentOffColor => style == null ? Color.gray : style.OffColor;

#if UNITY_EDITOR
        public void EditorConfigure(int index, LightsOutController owner,
            SpriteRenderer art, SwitchStyle assignedStyle, ParticleSystem burst)
        {
            switchIndex = index;
            puzzle = owner;
            plateRenderer = art;
            style = assignedStyle;
            sparks = burst;
        }
#endif
    }
}
