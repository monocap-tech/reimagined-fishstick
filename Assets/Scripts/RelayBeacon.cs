using UnityEngine;

namespace RelayYard
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RelayBeacon : MonoBehaviour
    {
        [SerializeField] private string relayId;
        [SerializeField] private RelayDefinition definition;
        [SerializeField] private RelayController controller;
        [SerializeField] private SpriteRenderer indicator;
        [SerializeField] private ParticleSystem completionFx;
        private bool activated;

        public string RelayId => relayId;

        private void Awake()
        {
            if (indicator != null)
                indicator.color = new Color(0.77f, 0.38f, 0.24f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || other.GetComponent<PlayerMotor>() == null)
                return;
            if (!controller.Activate(relayId))
                return;

            activated = true;
            if (indicator != null && definition != null)
                indicator.color = definition.ActiveColor;
            if (completionFx != null)
                completionFx.Play();
            if (AudioDirector.Instance != null)
                AudioDirector.Instance.PlayRelay();
        }

#if UNITY_EDITOR
        public void EditorConfigure(string id, RelayDefinition data, RelayController puzzle,
            SpriteRenderer renderer, ParticleSystem fx)
        {
            relayId = id;
            definition = data;
            controller = puzzle;
            indicator = renderer;
            completionFx = fx;
        }
#endif
    }
}
