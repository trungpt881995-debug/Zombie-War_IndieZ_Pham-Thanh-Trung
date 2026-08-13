using ZombieWar.Features.GameState.Domain;

namespace ZombieWar.Features.GameState.Model
{
    public sealed class GameStateModel
    {
        public GameplayStateId Current { get; private set; } = GameplayStateId.Inactive;
        public GameplayStateId Previous { get; private set; } = GameplayStateId.Inactive;
        public long TransitionSequence { get; private set; }

        public void Reset()
        {
            Current = GameplayStateId.Inactive;
            Previous = GameplayStateId.Inactive;
            TransitionSequence = 0;
        }

        internal void Commit(GameplayStateId next)
        {
            Previous = Current;
            Current = next;
            checked { TransitionSequence++; }
        }

        public GameplayStateSnapshot Snapshot() =>
            new GameplayStateSnapshot(Current, Previous, TransitionSequence);
    }
}
