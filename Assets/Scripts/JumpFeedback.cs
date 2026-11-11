using UnityEngine;

namespace RelayYard
{
    // Animation STATE callback, rather than a direct jump-button sound.
    public sealed class JumpFeedback : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            PlayerMotor player = animator.GetComponentInParent<PlayerMotor>();
            if (player != null)
                player.OnJumpAnimationStarted();
        }
    }
}
