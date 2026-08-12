using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Ports
{
    public sealed class NullProjectileExplosionPort : IProjectileExplosionPort
    {
        public static readonly NullProjectileExplosionPort Instance = new NullProjectileExplosionPort();
        private NullProjectileExplosionPort() { }
        public void Explode(in ProjectileExplosionRequest request) { }
    }
}
