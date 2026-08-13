using GeneralCore.Architecture;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Events
{
    public readonly struct ScoreRunStartedEvent : IEvent
    {
        public ScoreSnapshot Snapshot { get; }
        public ScoreRunStartedEvent(in ScoreSnapshot snapshot) => Snapshot = snapshot;
    }
}
