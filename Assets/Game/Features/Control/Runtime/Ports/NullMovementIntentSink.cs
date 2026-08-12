using ZombieWar.Features.Control.Domain;

namespace ZombieWar.Features.Control.Ports
{
    public sealed class NullMovementIntentSink : IMovementIntentSink
    {
        public static readonly NullMovementIntentSink Instance = new NullMovementIntentSink();

        private NullMovementIntentSink() { }

        public void Set(in MovementIntent intent) { }
    }
}
