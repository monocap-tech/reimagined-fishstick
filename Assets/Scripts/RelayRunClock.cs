using UnityEngine;

namespace RelayYard
{
    /// <summary>
    /// One clock per playable Game scene. Only successful goal entries set a best
    /// time; there is no attempt counter, score or ranking system.
    /// Time.deltaTime automatically excludes paused frames (timeScale == 0).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RelayRunClock : MonoBehaviour
    {
        private const string BestKey = "RelayYard.BestClearSeconds";

        public static RelayRunClock Active { get; private set; }
        public static bool HasLastClear { get; private set; }
        public static float LastClearSeconds { get; private set; }
        public static bool LastWasRecord { get; private set; }

        public static bool HasBest => PlayerPrefs.HasKey(BestKey);
        public static float BestSeconds => PlayerPrefs.GetFloat(BestKey, 0f);

        public float ElapsedSeconds { get; private set; }
        public bool IsComplete { get; private set; }

        private void Awake()
        {
            Active = this;
            HasLastClear = false;
            LastWasRecord = false;
            ElapsedSeconds = 0f;
            IsComplete = false;
        }

        private void Update()
        {
            if (!IsComplete)
                ElapsedSeconds += Time.deltaTime;
        }

        public void CompleteAtGoal()
        {
            if (IsComplete) return;
            IsComplete = true;
            LastClearSeconds = ElapsedSeconds;
            HasLastClear = true;
            LastWasRecord = !HasBest || LastClearSeconds < BestSeconds;
            if (LastWasRecord)
            {
                PlayerPrefs.SetFloat(BestKey, LastClearSeconds);
                PlayerPrefs.Save();
            }
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }

        public static string FormatTime(float seconds)
        {
            int centiseconds = Mathf.Max(0, Mathf.FloorToInt(seconds * 100f));
            int minutes = centiseconds / 6000;
            int remainder = centiseconds % 6000;
            return string.Format("{0:00}:{1:00}.{2:00}",
                minutes, remainder / 100, remainder % 100);
        }
    }
}
