using UnityEngine;

namespace NeonRange
{
    /// <summary>Moves upward and returns by deactivation after a hit, wall contact, or timeout.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 12f;
        [SerializeField, Min(0.1f)] private float lifetime = 2f;
        private Rigidbody2D body;
        private float expiresAt;
        private bool inFlight;
        private void Awake() => body = GetComponent<Rigidbody2D>();

        public void Launch(Vector2 position)
        {
            // An inactive prefab may not have received Awake yet.
            if (body == null) body = GetComponent<Rigidbody2D>();
            body.position = position;
            transform.position = new Vector3(position.x, position.y, 0f);
            expiresAt = Time.time + lifetime;
            inFlight = true;
            gameObject.SetActive(true);
            body.linearVelocity = Vector2.up * speed;
        }
        private void Update()
        {
            if (inFlight && Time.time >= expiresAt) ReturnToPool();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!inFlight || Time.timeScale == 0f) return;
            if (other.CompareTag("Enemy"))
            {
                EnemyTarget target = other.GetComponent<EnemyTarget>();
                if (target != null)
                {
                    ReturnToPool(); // Guard against multiple callbacks from the same shot.
                    target.Hit();
                }
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("ArenaWall"))
                ReturnToPool();
        }
        private void ReturnToPool()
        {
            inFlight = false;
            body.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
        private void OnDisable() => inFlight = false;
    }
}
