namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileViewLaunchData
    {
        public ProjectilePoint Origin { get; }
        public ProjectileVector InitialVelocity { get; }
        public bool UseGravity { get; }

        public ProjectileViewLaunchData(in ProjectilePoint origin, in ProjectileVector initialVelocity, bool useGravity)
        {
            Origin = origin;
            InitialVelocity = initialVelocity;
            UseGravity = useGravity;
        }
    }
}
