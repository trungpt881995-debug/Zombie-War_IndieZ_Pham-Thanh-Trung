using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Model;

namespace ZombieWar.Features.Projectile.Impact
{
    public sealed class PiercingImpactPolicy : IProjectileImpactPolicy
    {
        public ProjectileImpactDecision Evaluate(ProjectileModel projectile, in ProjectileCollision collision)
        {
            switch (collision.Kind)
            {
                case ProjectileCollisionKind.Damageable:
                    if (projectile.HasAlreadyHit(collision.TargetId))
                        return new ProjectileImpactDecision(ProjectileImpactAction.Ignore);
                        
                    return new ProjectileImpactDecision(ProjectileImpactAction.DamageAndContinue);

                case ProjectileCollisionKind.Ground:
                case ProjectileCollisionKind.Environment:
                    return new ProjectileImpactDecision(ProjectileImpactAction.Complete, ProjectileEndReason.EnvironmentHit);

                default:
                    return new ProjectileImpactDecision(ProjectileImpactAction.Ignore);
            }
        }
    }
}
