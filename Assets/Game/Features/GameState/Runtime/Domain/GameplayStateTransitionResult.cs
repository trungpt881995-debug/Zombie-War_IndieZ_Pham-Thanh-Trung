namespace ZombieWar.Features.GameState.Domain
{
    public readonly struct GameplayStateTransitionResult
    {
        public bool Accepted { get; }
        public GameplayStateId Previous { get; }
        public GameplayStateId Current { get; }
        public GameplayStateTransitionReason Reason { get; }
        public GameplayStateTransitionFailure Failure { get; }

        private GameplayStateTransitionResult(
            bool accepted,
            GameplayStateId previous,
            GameplayStateId current,
            GameplayStateTransitionReason reason,
            GameplayStateTransitionFailure failure)
        {
            Accepted = accepted;
            Previous = previous;
            Current = current;
            Reason = reason;
            Failure = failure;
        }

        public static GameplayStateTransitionResult AcceptedTransition(
            GameplayStateId previous,
            GameplayStateId current,
            GameplayStateTransitionReason reason) =>
            new GameplayStateTransitionResult(true, previous, current, reason, GameplayStateTransitionFailure.None);

        public static GameplayStateTransitionResult Rejected(
            GameplayStateId current,
            GameplayStateTransitionReason reason,
            GameplayStateTransitionFailure failure) =>
            new GameplayStateTransitionResult(false, current, current, reason, failure);
    }
}
