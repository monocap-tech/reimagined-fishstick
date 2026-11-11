using System;
using System.Collections.Generic;

namespace RelayYard
{
    // Pure C# model: no MonoBehaviour, UnityEngine, or scene polling.
    public sealed class RelayLedger
    {
        private readonly HashSet<string> expected;
        private readonly HashSet<string> completed = new HashSet<string>();

        public RelayLedger(IEnumerable<string> relayIds)
        {
            expected = new HashSet<string>(relayIds);
            if (expected.Count == 0)
                throw new ArgumentException("A level needs at least one relay.", nameof(relayIds));
        }

        public int CompletedCount => completed.Count;
        public int RequiredCount => expected.Count;
        public bool IsSolved => completed.Count == expected.Count;

        public bool TryActivate(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && expected.Contains(id) && completed.Add(id);
        }
    }
}
