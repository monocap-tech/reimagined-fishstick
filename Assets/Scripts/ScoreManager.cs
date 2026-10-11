using UnityEngine;
using UnityEngine.UI;

namespace NeonRange
{
    /// <summary>Updates the run score and saves each new record immediately using PlayerPrefs.</summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        public const string BestScoreKey = "NeonRange.BestScore";
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        private int score;
        private int bestScore;
        private void Awake()
        {
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            Refresh();
        }
        public void AddPoint()
        {
            score++;
            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt(BestScoreKey, bestScore);
                PlayerPrefs.Save();
            }
            Refresh();
        }
        private void Refresh()
        {
            scoreText.text = "Score: " + score;
            bestScoreText.text = "Best: " + bestScore;
        }
    }
}
