using UnityEngine;
using UnityEngine.UI;

namespace NeonRange
{
    /// <summary>Reloads the saved record whenever the Main Menu opens.</summary>
    public sealed class MenuBestScore : MonoBehaviour
    {
        [SerializeField] private Text bestScoreText;
        private void OnEnable() => bestScoreText.text = "Best: " +
            PlayerPrefs.GetInt(ScoreManager.BestScoreKey, 0);
    }
}
