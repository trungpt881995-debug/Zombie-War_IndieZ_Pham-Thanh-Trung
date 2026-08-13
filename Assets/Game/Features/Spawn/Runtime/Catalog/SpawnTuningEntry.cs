using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Catalog
{
    public readonly struct SpawnTuningEntry
    {
        public SpawnDifficultyKey Key { get; }
        public SpawnTuning Tuning { get; }
        public SpawnTuningEntry(in SpawnDifficultyKey key,in SpawnTuning tuning) { Key=key; Tuning=tuning; }
    }
}
