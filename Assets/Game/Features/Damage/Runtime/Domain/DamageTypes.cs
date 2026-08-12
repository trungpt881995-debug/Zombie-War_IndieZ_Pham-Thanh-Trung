namespace ZombieWar.Features.Damage.Domain
{
    /// <summary>
    /// Stable, allocation-free identifiers for the damage delivery mechanism.
    /// Weapon-specific balance remains in Weapon/Zombie/Boss config, not here.
    /// </summary>
    public static class DamageTypes
    {
        public const string Projectile = "Projectile";
        public const string Area = "Area";
        public const string FlameTick = "FlameTick";
        public const string ZombieAttack = "ZombieAttack";
        public const string BossAttack = "BossAttack";
    }
}
