using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Model;

namespace ZombieWar.Features.Projectile.Impact
{
    public sealed class StopOnHitPolicy : IProjectileImpactPolicy
    {
        public ProjectileImpactDecision Evaluate(ProjectileModel projectile, in ProjectileCollision collision)
        {
            switch (collision.Kind)
            {
                case ProjectileCollisionKind.Damageable:
                    return new ProjectileImpactDecision(ProjectileImpactAction.DamageAndComplete, ProjectileEndReason.Hit);

                case ProjectileCollisionKind.Ground:
                case ProjectileCollisionKind.Environment:
                    return new ProjectileImpactDecision(ProjectileImpactAction.Complete, ProjectileEndReason.EnvironmentHit);
                    
                default:
                    return new ProjectileImpactDecision(ProjectileImpactAction.Ignore);
            }
        }
    }
}
