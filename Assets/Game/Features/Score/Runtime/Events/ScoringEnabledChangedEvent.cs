using GeneralCore.Architecture;

namespace ZombieWar.Features.Score.Events
{
    public readonly struct ScoringEnabledChangedEvent : IEvent
    {
        public bool Enabled { get; }
        public ScoringEnabledChangedEvent(bool enabled) => Enabled = enabled;
    }
}
