using System;
using UnityEngine;

namespace RelayYard
{
    // Switches report contacts; this controller alone updates the logic and presentation.
    public sealed class LightsOutController : MonoBehaviour
    {
        [SerializeField] private PressurePlate[] plates;
        [SerializeField] private SpriteRenderer[] indicators;
        [SerializeField] private ExitGate exitGate;
        [SerializeField] private ParticleSystem solveBurst;
        [SerializeField] private SeededSignal signalDisplay;
        [SerializeField] private int initialMask = 20; // OFF OFF ON OFF ON OFF

        private LightsOutBoard board;
        public int LitCount => board == null ? 0 : board.LitCount;
        public int TotalCount => board == null ? 6 : board.Count;
        public bool IsSolved => board != null && board.IsSolved;
        public int PressCount { get; private set; }
        public event Action OnSolved;

        private void Awake()
        {
            board = new LightsOutBoard(6, initialMask);
            RefreshIndicators();
        }

        public bool Press(int index)
        {
            if (board == null || board.IsSolved || index < 0 || index >= board.Count)
                return false;
            board.Press(index);
            PressCount++;
            RefreshIndicators();
            if (board.IsSolved)
            {
                if (exitGate != null) exitGate.Unlock();
                if (solveBurst != null) solveBurst.Emit(45);
                if (signalDisplay != null) signalDisplay.SetSolved(true);
                if (AudioDirector.Instance != null)
                    AudioDirector.Instance.PlayUnlock();
                OnSolved?.Invoke();
            }
            return true;
        }

        private void RefreshIndicators()
        {
            if (board == null) return;
            for (int i = 0; i < board.Count; i++)
            {
                bool lit = board.IsLit(i);
                if (plates != null && i < plates.Length && plates[i] != null)
                    plates[i].ShowState(lit);
                if (indicators != null && i < indicators.Length && indicators[i] != null)
                {
                    PressurePlate plate = plates != null && i < plates.Length ? plates[i] : null;
                    indicators[i].color = plate == null
                        ? (lit ? Color.yellow : Color.gray)
                        : (lit ? plate.CurrentOnColor : plate.CurrentOffColor);
                }
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(PressurePlate[] assignedPlates, SpriteRenderer[] lamps,
            ExitGate gate, ParticleSystem winFx, SeededSignal background)
        {
            plates = assignedPlates;
            indicators = lamps;
            exitGate = gate;
            solveBurst = winFx;
            signalDisplay = background;
            initialMask = 20;
        }
#endif
    }
}
