namespace ZombieWar.Features.UI.Utilities
{
    public sealed class SingleExecutionGuard
    {
        public bool IsLocked
        {
            get;
            private set;
        }
        public bool TryEnter()
        {
            if (IsLocked) return false;
            IsLocked = true;
            return true;
        }
        public void Reset() => IsLocked = false;
    }
}
