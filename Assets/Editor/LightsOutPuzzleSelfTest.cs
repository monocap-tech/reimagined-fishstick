#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RelayYard.EditorTools
{
    // Editor-only deterministic checks; not a substitute for real gameplay testing.
    public static class LightsOutPuzzleSelfTest
    {
        [MenuItem("Tools/Relay Yard/Validate Six-Switch Logic")]
        public static void Validate()
        {
            var board = new LightsOutBoard(6, 20);
            board.Press(1); // switch 2
            board.Press(3); // switch 4
            board.Press(5); // switch 6
            if (!board.IsSolved || board.LitCount != 6)
            {
                Debug.LogError("[Lights Out] Expected 2,4,6 to solve initial state.");
                return;
            }

            // Every initial configuration is reachable from the all-on state
            // by some sequence of neighbor toggles for this six-light row.
            var visited = new bool[64];
            var queue = new Queue<int>();
            visited[63] = true;
            queue.Enqueue(63);
            while (queue.Count != 0)
            {
                int mask = queue.Dequeue();
                for (int i = 0; i < 6; i++)
                {
                    int next = mask;
                    for (int j = Mathf.Max(0, i - 1); j <= Mathf.Min(5, i + 1); j++)
                        next ^= 1 << j;
                    if (visited[next]) continue;
                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }
            for (int mask = 0; mask < visited.Length; mask++)
                if (!visited[mask])
                {
                    Debug.LogError("[Lights Out] Unreachable six-light pattern: " + mask);
                    return;
                }

            // Stepping on the same plate twice should restore the state.
            var reversibility = new LightsOutBoard(6, 20);
            reversibility.Press(2);
            reversibility.Press(2);
            if (reversibility.LitCount != 2 ||
                !reversibility.IsLit(2) || !reversibility.IsLit(4))
            {
                Debug.LogError("[Lights Out] A repeated switch press did not reverse the state.");
                return;
            }
            Debug.Log("[Lights Out] PASS: known 2-4-6 solution, all 64 configurations reachable, double press reversible. Physical plate traversal must still be playtested.");
        }
    }
}
#endif
