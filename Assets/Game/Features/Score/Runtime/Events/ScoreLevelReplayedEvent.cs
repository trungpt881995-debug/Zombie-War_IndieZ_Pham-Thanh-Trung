using GeneralCore.Architecture;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Events
{
    public readonly struct ScoreLevelReplayedEvent : IEvent
    {
        public long PreviousTotal { get; }
        public ScoreSnapshot Snapshot { get; }

        public ScoreLevelReplayedEvent(long previousTotal, in ScoreSnapshot snapshot)
        {
            PreviousTotal = previousTotal;
            Snapshot = snapshot;
        }
    }
}
