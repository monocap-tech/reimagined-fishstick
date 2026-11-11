using UnityEngine;
using UnityEngine.SceneManagement;

namespace RelayYard
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ExitGate : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer indicator;
        private bool unlocked;

        public void Unlock()
        {
            unlocked = true;
            if (indicator != null)
                indicator.color = new Color(0.15f, 0.95f, 0.48f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (unlocked && other.GetComponent<PlayerMotor>() != null)
                SceneManager.LoadScene("Win");
        }

#if UNITY_EDITOR
        public void EditorConfigure(SpriteRenderer renderer)
        {
            indicator = renderer;
        }
#endif
    }
}
