using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats",
    menuName = "Enemies/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public string enemyName;
    public int maxHealth = 30;
    public float moveSpeed = 1.5f;
    public Color color = Color.white;
}
