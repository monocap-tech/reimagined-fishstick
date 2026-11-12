using System;

namespace RelayYard
{
    // Pure C#: the six-light puzzle has no Unity or scene dependencies.
    // Pressing a switch toggles itself and its immediate horizontal neighbors.
    public sealed class LightsOutBoard
    {
        private readonly bool[] lights;
        public int Count => lights.Length;
        public int LitCount { get; private set; }
        public bool IsSolved => LitCount == lights.Length;

        public LightsOutBoard(int count, int initialMask)
        {
            if (count < 2 || count > 30)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (initialMask < 0 || (initialMask >> count) != 0)
                throw new ArgumentOutOfRangeException(nameof(initialMask));
            lights = new bool[count];
            for (int i = 0; i < count; i++)
            {
                lights[i] = (initialMask & (1 << i)) != 0;
                if (lights[i]) LitCount++;
            }
        }

        public bool IsLit(int index)
        {
            if (index < 0 || index >= lights.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return lights[index];
        }

        public void Press(int index)
        {
            if (index < 0 || index >= lights.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            for (int i = Math.Max(0, index - 1); i <= Math.Min(lights.Length - 1, index + 1); i++)
            {
                lights[i] = !lights[i];
                LitCount += lights[i] ? 1 : -1;
            }
        }
    }
}
