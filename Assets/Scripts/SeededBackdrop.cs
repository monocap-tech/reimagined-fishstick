using System;
using UnityEngine;

namespace RelayYard
{
    public sealed class SeededBackdrop : MonoBehaviour
    {
        [Header("Editable procedural-generation parameters")]
        [SerializeField] private int seed = 386;
        [SerializeField, Range(5, 80)] private int starCount = 36;
        [SerializeField] private Sprite dotSprite;

        private void Start()
        {
            if (dotSprite == null) return;
            var random = new System.Random(seed);
            for (int i = 0; i < starCount; i++)
            {
                float x = (float)random.NextDouble() * 16.5f - 8.25f;
                float y = (float)random.NextDouble() * 6.1f - 2.0f;
                float size = 0.035f + (float)random.NextDouble() * 0.11f;
                GameObject star = new GameObject("Generated Star " + i);
                star.transform.SetParent(transform, false);
                star.transform.localPosition = new Vector3(x, y, 0);
                star.transform.localScale = new Vector3(size, size, 1);
                var sprite = star.AddComponent<SpriteRenderer>();
                sprite.sprite = dotSprite;
                sprite.color = new Color(0.43f, 0.65f, 0.87f, 0.8f);
                sprite.sortingOrder = -10;
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(Sprite sprite, int initialSeed)
        {
            dotSprite = sprite;
            seed = initialSeed;
        }
#endif
    }
}
