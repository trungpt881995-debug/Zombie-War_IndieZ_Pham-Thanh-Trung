using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Domain;

namespace ZombieWar.Features.GameState.Events
{
    public readonly struct GameplayStateChangedEvent : IEvent
    {
        public GameplayStateId Previous { get; }
        public GameplayStateId Current { get; }
        public GameplayStateTransitionReason Reason { get; }
        public long Sequence { get; }

        public GameplayStateChangedEvent(
            GameplayStateId previous,
            GameplayStateId current,
            GameplayStateTransitionReason reason,
            long sequence)
        {
            Previous = previous;
            Current = current;
            Reason = reason;
            Sequence = sequence;
        }
    }
}
