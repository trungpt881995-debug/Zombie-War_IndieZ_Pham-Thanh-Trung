using GeneralCore.Architecture;
using GameplayCore.Damage;
using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Impact;
using ZombieWar.Features.Projectile.Model;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Registry;

namespace ZombieWar.Features.Projectile.Factories
{
    public sealed class ProjectileControllerFactory : IProjectileControllerFactory
    {
        private readonly IDamageService _damage;
        private readonly IProjectileImpactPolicyProvider _impactPolicies;
        private readonly IProjectileExplosionPort _explosion;
        private readonly IProjectileFeedbackPort _feedback;
        private readonly IEventBus _events;

        public ProjectileControllerFactory(IDamageService damage, IProjectileImpactPolicyProvider impactPolicies, IProjectileExplosionPort explosion, IProjectileFeedbackPort feedback, IEventBus events)
        {
            _damage = damage;
            _impactPolicies = impactPolicies;
            _explosion = explosion;
            _feedback = feedback;
            _events = events;
        }

        public ProjectileController Create(IProjectileView view, IProjectilePool pool, IActiveProjectileRegistry registry)
        {
            return new ProjectileController(
                new ProjectileModel(),
                view,
                _damage,
                _impactPolicies,
                _explosion,
                _feedback,
                pool,
                registry,
                _events
                );
        }
    }
}
