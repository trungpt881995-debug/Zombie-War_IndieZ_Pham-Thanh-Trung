using System;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectilePoolKey : IEquatable<ProjectilePoolKey>
    {
        public int Value { get; }

        public ProjectilePoolKey(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }

        public bool Equals(ProjectilePoolKey other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ProjectilePoolKey other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
        public static bool operator ==(ProjectilePoolKey left, ProjectilePoolKey right) => left.Equals(right);
        public static bool operator !=(ProjectilePoolKey left, ProjectilePoolKey right) => !left.Equals(right);
    }
}
