using UnityEngine;

namespace RelayYard
{
    // Week 9-style Perlin noise: deterministic seed and Inspector-tunable scale,
    // combined with a traveling sine wave like a small retro media visualizer.
    // This is decorative; it never influences the six-switch solution.
    public sealed class SeededSignal : MonoBehaviour
    {
        [SerializeField] private int seed = 386;
        [SerializeField, Range(24, 160)] private int sampleCount = 96;
        [SerializeField, Range(0.2f, 10f)] private float noiseScale = 3.8f;
        [SerializeField, Range(0f, 1f)] private float noiseStrength = 0.24f;
        [SerializeField, Range(0f, 2f)] private float waveAmplitude = 0.24f;
        [SerializeField, Range(0f, 5f)] private float speed = 0.65f;

        private LineRenderer line;
        private bool solved;
        private float startTime;

        private void Awake()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = sampleCount;
            line.widthMultiplier = 0.035f;
            line.numCornerVertices = 2;
            line.sortingOrder = -5;
            // URP 2D draws the procedural trace with a compatible unlit shader.
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader != null)
                line.material = new Material(shader);
            startTime = Time.time;
        }

        public void SetSolved(bool value) => solved = value;

        private void Update()
        {
            if (line == null) return;
            Color color = solved ? new Color(0.45f, 1f, 0.78f, 0.80f)
                : new Color(0.35f, 0.70f, 0.96f, 0.52f);
            line.startColor = color;
            line.endColor = color;
            float t = (Time.time - startTime) * speed;
            int count = Mathf.Max(2, sampleCount);
            if (line.positionCount != count) line.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                float u = i / (float)(count - 1);
                float x = Mathf.Lerp(-7.7f, 7.7f, u);
                float n = Mathf.PerlinNoise((x + seed) / noiseScale, (t + seed) / noiseScale);
                float wave = Mathf.Sin(x * 2.4f - t * 3.1f) * waveAmplitude;
                float y = wave + (n - 0.5f) * 2f * noiseStrength;
                line.SetPosition(i, new Vector3(x, y, 0));
            }
        }

        private void OnDestroy()
        {
            if (line != null && line.material != null)
                Destroy(line.material);
        }

#if UNITY_EDITOR
        public void EditorConfigure(int assignedSeed) => seed = assignedSeed;
#endif
    }
}
