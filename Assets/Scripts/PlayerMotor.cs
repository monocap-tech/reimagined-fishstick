using UnityEngine;
using UnityEngine.InputSystem;

namespace RelayYard
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Input System")]
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private float runSpeed = 5.4f;
        [SerializeField] private float jumpVelocity = 9.6f;

        [Header("Ground contact")]
        [SerializeField] private Transform groundProbe;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float probeRadius = 0.13f;

        [Header("Feedback")]
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem takeoffParticles;

        private Rigidbody2D body;
        private InputActionMap gameplay;
        private InputAction move;
        private InputAction jump;
        private float moveInput;
        private bool jumpQueued;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            gameplay = controls.FindActionMap("Gameplay", true);
            move = gameplay.FindAction("Move", true);
            jump = gameplay.FindAction("Jump", true);
        }

        private void OnEnable() => gameplay?.Enable();
        private void OnDisable() => gameplay?.Disable();

        private void Update()
        {
            moveInput = move.ReadValue<float>();
            if (jump.WasPressedThisFrame()) jumpQueued = true;

            if (animator != null)
            {
                float visualSpeed = Mathf.MoveTowards(animator.GetFloat("Speed"),
                    Mathf.Abs(moveInput), Time.deltaTime * 4f);
                animator.SetFloat("Speed", visualSpeed);
                animator.SetFloat("VerticalSpeed", body.linearVelocity.y);
                animator.SetBool("Grounded", IsGrounded());
            }
        }

        private void FixedUpdate()
        {
            body.linearVelocity = new Vector2(moveInput * runSpeed, body.linearVelocity.y);
            if (jumpQueued && IsGrounded())
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
            jumpQueued = false;
        }

        private bool IsGrounded()
        {
            return groundProbe != null &&
                   Physics2D.OverlapCircle(groundProbe.position, probeRadius, groundMask) != null;
        }

        public void OnJumpAnimationStarted()
        {
            if (takeoffParticles != null) takeoffParticles.Emit(12);
            if (AudioDirector.Instance != null) AudioDirector.Instance.PlayJump();
        }

#if UNITY_EDITOR
        public void EditorConfigure(InputActionAsset actions, Transform probe, LayerMask ground,
            Animator assignedAnimator, ParticleSystem particles)
        {
            controls = actions;
            groundProbe = probe;
            groundMask = ground;
            animator = assignedAnimator;
            takeoffParticles = particles;
        }
#endif
    }
}
