using UnityEngine;

namespace RelayYard
{
    // Solid gate rises out of the way on the solved event. A separate goal trigger
    // loads the Win scene, matching the midterm's gate/goal separation.
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ExitGate : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer gateVisual;
        [SerializeField] private SpriteRenderer halo;
        [SerializeField] private Transform player;
        [SerializeField] private float liftDistance = 5f;
        [SerializeField] private float liftSpeed = 3f;

        private Vector3 closedPosition;
        private bool unlocked;
        [SerializeField] private bool use2DLighting;
        // Exit eligibility changes the instant the sixth indicator becomes lit.
        // The lifting animation is presentation, not an additional win condition.
        public bool IsOpen => unlocked;

        private void Awake() => closedPosition = transform.position;

        public void Unlock()
        {
            if (unlocked) return;
            unlocked = true;
            // Allow passage immediately instead of waiting for the visible gate
            // to clear the doorway over several Update frames.
            BoxCollider2D barrier = GetComponent<BoxCollider2D>();
            if (barrier != null) barrier.enabled = false;
        }

        private void Update()
        {
            if (unlocked)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    closedPosition + Vector3.up * liftDistance, liftSpeed * Time.deltaTime);
            }

            // In the URP branch the real Light 2D is carried by the player.
            // The gate's Sprite-Lit material receives light spatially; no fake
            // distance-based tinting or rectangular halo is used.
            if (use2DLighting)
            {
                if (gateVisual != null)
                    gateVisual.color = unlocked
                        ? new Color(0.34f, 0.78f, 0.59f)
                        : new Color(0.57f, 0.46f, 0.36f);
                return;
            }

            float proximity = player == null ? 0f :
                Mathf.Clamp01(1f - Vector2.Distance(player.position, transform.position) / 4.5f);
            if (gateVisual != null)
                gateVisual.color = unlocked
                    ? new Color(0.28f, 0.9f, 0.65f)
                    : Color.Lerp(new Color(0.40f, 0.23f, 0.18f),
                        new Color(1f, 0.55f, 0.18f), proximity);
            if (halo != null)
            {
                Color c = unlocked ? new Color(0.25f, 1f, 0.66f, 0.17f)
                    : new Color(1f, 0.48f, 0.12f, 0.04f + proximity * 0.26f);
                halo.color = c;
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(SpriteRenderer main, SpriteRenderer glow, Transform target)
        {
            gateVisual = main;
            halo = glow;
            player = target;
        }

        public void EditorEnableURP2DLighting()
        {
            use2DLighting = true;
            if (halo != null) halo.gameObject.SetActive(false);
        }
#endif
    }
}
