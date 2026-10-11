using System;

public class Health
{
    public int Max { get; private set; }
    public int Current { get; private set; }
    public bool IsDead { get { return Current <= 0; } }

    public event Action Died;

    public Health(int max)
    {
        Max = max;
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        Current = Math.Max(0, Current - amount);
        if (IsDead && Died != null) Died();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        Current = Math.Min(Max, Current + amount);
    }
}
