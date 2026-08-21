using System;
namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnDifficultyKey : IEquatable<SpawnDifficultyKey>
    {
        public int GameLevel { get; }
        public int SoldierGroupLevel { get; }
        public SpawnDifficultyKey(int gameLevel, int soldierGroupLevel)
        {
            if (gameLevel <= 0) throw new ArgumentOutOfRangeException(nameof(gameLevel));
            if (soldierGroupLevel <= 0) throw new ArgumentOutOfRangeException(nameof(soldierGroupLevel));
            GameLevel=gameLevel; 
            SoldierGroupLevel=soldierGroupLevel;
        }
        public bool Equals(SpawnDifficultyKey other) => GameLevel==other.GameLevel && SoldierGroupLevel==other.SoldierGroupLevel;
        public override bool Equals(object obj) => obj is SpawnDifficultyKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(GameLevel,SoldierGroupLevel);
        public override string ToString() => $"GameLevel{GameLevel}/SoldierLevel{SoldierGroupLevel}";
    }
}
