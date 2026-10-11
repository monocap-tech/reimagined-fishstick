using UnityEngine;

namespace NeonRange
{
    /// <summary>A reusable target that scores one hit and respawns inside its assigned lane.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyTarget : MonoBehaviour
    {
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private Vector2 minimum = new Vector2(-5f, 1f);
        [SerializeField] private Vector2 maximum = new Vector2(-2f, 3f);
        public void Hit()
        {
            scoreManager.AddPoint();
            transform.position = new Vector3(Random.Range(minimum.x, maximum.x),
                Random.Range(minimum.y, maximum.y), 0f);
        }
    }
}
