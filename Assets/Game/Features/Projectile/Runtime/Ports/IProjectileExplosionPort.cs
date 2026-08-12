using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Ports
{
    public interface IProjectileExplosionPort
    {
        void Explode(in ProjectileExplosionRequest request);
    }
}
