using System.Collections.Generic;
using UnityEngine;

namespace RelayYard
{
    public sealed class RelayController : MonoBehaviour
    {
        [SerializeField] private RelayBeacon[] relays;
        [SerializeField] private ExitGate exitGate;
        private RelayLedger ledger;

        public int ActiveCount => ledger == null ? 0 : ledger.CompletedCount;
        public int TotalCount => ledger == null ? 0 : ledger.RequiredCount;
        public bool IsSolved => ledger != null && ledger.IsSolved;

        private void Awake()
        {
            var ids = new List<string>();
            foreach (RelayBeacon relay in relays)
                if (relay != null) ids.Add(relay.RelayId);
            ledger = new RelayLedger(ids);
        }

        public bool Activate(string relayId)
        {
            if (!ledger.TryActivate(relayId))
                return false;

            if (ledger.IsSolved)
            {
                exitGate.Unlock();
                if (AudioDirector.Instance != null)
                    AudioDirector.Instance.PlayUnlock();
            }
            return true;
        }

#if UNITY_EDITOR
        public void EditorConfigure(RelayBeacon[] assignedRelays, ExitGate assignedExit)
        {
            relays = assignedRelays;
            exitGate = assignedExit;
        }
#endif
    }
}
