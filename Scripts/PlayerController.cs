using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonRange
{
    /// <summary>Reads asset-backed input; moves with 2D physics and fires pooled bullets.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private Transform muzzle;
        [SerializeField, Min(0.1f)] private float speed = 5f;
        [SerializeField, Min(0.05f)] private float fireInterval = 0.18f;
        private Rigidbody2D body;
        private Vector2 movement;
        private float nextFireTime;

        private void Awake() => body = GetComponent<Rigidbody2D>();
        private void OnEnable()
        {
            moveAction.action.Enable();
            fireAction.action.Enable();
        }
        private void Update()
        {
            if (Time.timeScale == 0f) { movement = Vector2.zero; return; }
            movement = Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
            if (fireAction.action.IsPressed() && Time.time >= nextFireTime)
            {
                bulletPool.Fire(muzzle.position);
                nextFireTime = Time.time + fireInterval;
            }
        }
        private void FixedUpdate() => body.linearVelocity = movement * speed;
        private void OnDisable()
        {
            moveAction.action.Disable();
            fireAction.action.Disable();
            body.linearVelocity = Vector2.zero;
        }
    }
}
