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
        public bool IsOpen => unlocked && Vector3.Distance(transform.position,
            closedPosition + Vector3.up * liftDistance) < 0.04f;

        private void Awake() => closedPosition = transform.position;

        public void Unlock() => unlocked = true;

        private void Update()
        {
            if (unlocked)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    closedPosition + Vector3.up * liftDistance, liftSpeed * Time.deltaTime);
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
#endif
    }
}
