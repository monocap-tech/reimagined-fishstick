using UnityEngine;
using UnityEngine.SceneManagement;

namespace RelayYard
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ExitGoal : MonoBehaviour
    {
        [SerializeField] private ExitGate gate;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (gate != null && gate.IsOpen && other.GetComponent<PlayerMotor>() != null)
                SceneManager.LoadScene("Win");
        }

#if UNITY_EDITOR
        public void EditorConfigure(ExitGate assignedGate) => gate = assignedGate;
#endif
    }
}
