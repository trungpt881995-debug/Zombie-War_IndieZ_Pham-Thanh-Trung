using GeneralCore.Architecture;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Events
{
    public readonly struct ScoreLevelStartedEvent : IEvent
    {
        public ScoreSnapshot Snapshot { get; }
        public ScoreLevelStartedEvent(in ScoreSnapshot snapshot) => Snapshot = snapshot;
    }
}
