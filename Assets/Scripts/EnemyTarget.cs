
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonRange
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class EnemyTarget : MonoBehaviour
    {
        [Header("Existing Project 1")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private Vector2 minimum = new Vector2(-5f, 1f);
        [SerializeField] private Vector2 maximum = new Vector2(-2f, 3f);

        [Header("Enemy Stats and Health")]
        [SerializeField] private EnemyStats stats;
        [SerializeField] private Key damageKey = Key.Digit1;
        [SerializeField, Min(1)] private int damageAmount = 10;
        [SerializeField, Min(0f)] private float respawnDelay = 0.75f;

        private Health health;
        private SpriteRenderer sprite;
        private BoxCollider2D hitbox;
        private Vector3 fullScale;
        private float movementPhase;

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            hitbox = GetComponent<BoxCollider2D>();
            fullScale = transform.localScale;

            if (stats == null)
            {
                Debug.LogError(name + ": EnemyStats asset missing!", this);
                enabled = false;
                return;
            }

            CreateHealth();
        }

        private void CreateHealth()
        {
            if (health != null)
                health.Died -= OnDied;

            health = new Health(Mathf.Max(1, stats.maxHealth));
            health.Died += OnDied;

            hitbox.enabled = true;
            movementPhase = Random.Range(0f, 4f);
            Refresh();

            Debug.Log(name + " ready. Health "
                + health.Current + "/" + health.Max);
        }

        private void Update()
        {
            if (health == null || health.IsDead)
                return;

            // Move horizontally within the existing target lane.
            float left = Mathf.Min(minimum.x, maximum.x);
            float right = Mathf.Max(minimum.x, maximum.x);
            float width = right - left;

            if (width > 0f)
            {
                Vector3 pos = transform.position;
                pos.x = left + Mathf.PingPong(
                    Time.time * stats.moveSpeed + movementPhase,
                    width);
                transform.position = pos;
            }

            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb[damageKey].wasPressedThisFrame)
                TakeDamage(damageAmount);

            if (kb.hKey.wasPressedThisFrame && !health.IsDead)
            {
                health.Heal(damageAmount);
                Refresh();
            }
        }

        // Called by the existing Bullet.cs.
        public void Hit()
        {
            if (health == null || health.IsDead)
                return;

            if (scoreManager != null)
                scoreManager.AddPoint();

            TakeDamage(damageAmount);
        }

        private void TakeDamage(int amount)
        {
            health.TakeDamage(amount);
            Refresh();
        }

        private void Refresh()
        {
            float ratio = (float)health.Current / health.Max;

            transform.localScale = fullScale *
                (0.5f + 0.5f * ratio);

            sprite.color = health.IsDead
                ? Color.gray
                : stats.color;

            Debug.Log(name + " health: "
                + health.Current + "/" + health.Max);
        }

        private void OnDied()
        {
            hitbox.enabled = false;
            sprite.color = Color.gray;

            Debug.Log(name + " died.");
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);

            transform.position = new Vector3(
                Random.Range(minimum.x, maximum.x),
                Random.Range(minimum.y, maximum.y),
                0f);

            CreateHealth();
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= OnDied;
        }
    }
}
