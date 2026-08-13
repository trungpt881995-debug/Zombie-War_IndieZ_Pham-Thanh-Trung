namespace ZombieWar.Features.GameState.Domain
{
    public readonly struct GameplayStateSnapshot
    {
        public GameplayStateId Current { get; }
        public GameplayStateId Previous { get; }
        public long TransitionSequence { get; }

        public GameplayStateSnapshot(
            GameplayStateId current,
            GameplayStateId previous,
            long transitionSequence)
        {
            Current = current;
            Previous = previous;
            TransitionSequence = transitionSequence;
        }
    }
}
