using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Model;

namespace ZombieWar.Features.Projectile.Impact
{
    public sealed class ExplodeOnGroundPolicy : IProjectileImpactPolicy
    {
        public ProjectileImpactDecision Evaluate(ProjectileModel projectile, in ProjectileCollision collision)
        {
            switch (collision.Kind)
            {
                case ProjectileCollisionKind.Ground:
                    return new ProjectileImpactDecision(ProjectileImpactAction.ExplodeAndComplete, ProjectileEndReason.GroundExplosion);
                case ProjectileCollisionKind.Environment:
                    return new ProjectileImpactDecision(ProjectileImpactAction.Complete, ProjectileEndReason.EnvironmentHit);
                default:
                    return new ProjectileImpactDecision(ProjectileImpactAction.Ignore);
            }
        }
    }
}
