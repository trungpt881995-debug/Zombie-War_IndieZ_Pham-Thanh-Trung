using System;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Events;
using ZombieWar.Features.Projectile.Impact;
using ZombieWar.Features.Projectile.Model;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Registry;

namespace ZombieWar.Features.Projectile.Controller
{
    public sealed class ProjectileController : IController
    {
        private readonly ProjectileModel _model;
        private readonly IProjectileView _view;
        private readonly IDamageService _damageService;
        private readonly IProjectileImpactPolicyProvider _impactPolicies;
        private readonly IProjectileExplosionPort _explosion;
        private readonly IProjectileFeedbackPort _feedback;
        private readonly IProjectilePool _pool;
        private readonly IActiveProjectileRegistry _registry;
        private readonly IEventBus _events;
        private bool _releaseRequested;

        public ProjectileController(ProjectileModel model, IProjectileView view, IDamageService damageService, IProjectileImpactPolicyProvider impactPolicies, IProjectileExplosionPort explosion, IProjectileFeedbackPort feedback, IProjectilePool pool, IActiveProjectileRegistry registry, IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _damageService = damageService ?? throw new ArgumentNullException(nameof(damageService));
            _impactPolicies = impactPolicies ?? throw new ArgumentNullException(nameof(impactPolicies));
            _explosion = explosion ?? throw new ArgumentNullException(nameof(explosion));
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public IProjectileView View => _view;
        public bool IsFlying => _model.IsFlying;
        public EntityId ProjectileId => _model.ProjectileId;

        public void Launch(EntityId projectileId, in ProjectileLaunchRequest request, in ProjectileVector initialVelocity)
        {
            _releaseRequested = false;
            _model.Activate(projectileId, in request);
            ProjectilePoint origin = request.Origin;
            var viewData = new ProjectileViewLaunchData(in origin, in initialVelocity, request.MotionKind == ProjectileMotionKind.Ballistic);
            _view.Activate(in viewData);
            _registry.Add(this);
            _events.Publish(new ProjectileLaunchedEvent(projectileId, request.OwnerId, request.PoolKey));
        }

        public void Tick(float deltaTime)
        {
            if (!_model.IsFlying) return;
            ProjectilePoint position = _view.Position;
            _model.Advance(deltaTime, in position);

            if (_model.HasExpired)
            {
                Complete(ProjectileEndReason.LifetimeExpired);
                return;
            }

            if (_model.HasReachedMaxRange)
                Complete(ProjectileEndReason.MaxRangeReached);
        }

        public void HandleCollision(in ProjectileCollision collision)
        {
            if (!_model.IsFlying) return;

            IProjectileImpactPolicy policy = _impactPolicies.Get(_model.ImpactMode);
            ProjectileImpactDecision decision = policy.Evaluate(_model, in collision);

            switch (decision.Action)
            {
                case ProjectileImpactAction.Ignore:
                    return;

                case ProjectileImpactAction.DamageAndComplete: ApplyDirectDamage(in collision, registerHit: false);
                    Complete(decision.EndReason == ProjectileEndReason.None ? ProjectileEndReason.Hit : decision.EndReason);
                    return;

                case ProjectileImpactAction.DamageAndContinue:
                    if (!_model.RegisterHit(collision.TargetId)) return;
                    ApplyDirectDamage(in collision, registerHit: false);
                    return;

                case ProjectileImpactAction.Complete:
                    Complete(decision.EndReason == ProjectileEndReason.None ? ProjectileEndReason.EnvironmentHit : decision.EndReason);
                    return;

                case ProjectileImpactAction.ExplodeAndComplete : ProjectilePoint explosionPoint = collision.ContactPoint;
                    Explode(in explosionPoint);
                    Complete(decision.EndReason == ProjectileEndReason.None ? ProjectileEndReason.GroundExplosion : decision.EndReason);
                    return;
            }
        }

        public void Cancel()
        {
            if (_model.IsFlying)
                Complete(ProjectileEndReason.Cancelled);
        }

        public void ResetForPool()
        {
            _releaseRequested = false;
            _model.Reset();
        }

        private void ApplyDirectDamage(in ProjectileCollision collision, bool registerHit)
        {
            if (!collision.HasDamageable) return;
            if (registerHit && !_model.RegisterHit(collision.TargetId)) return;

            var info = new DamageInfo(_model.OwnerId, _model.Damage, "Projectile");

            if (_damageService.TryApply(collision.Damageable, info))
            {
                ProjectilePoint point = collision.ContactPoint;
                _feedback.OnHit(_model.ProjectileId, collision.TargetId, in point);
                _events.Publish(new ProjectileHitEvent(_model.ProjectileId, _model.OwnerId, collision.TargetId, _model.Damage));
            }
        }

        private void Explode(in ProjectilePoint point)
        {
            var request = new ProjectileExplosionRequest( _model.OwnerId, _model.ProjectileId, in point, _model.ExplosionRadius, _model.Damage);
            _explosion.Explode(in request);
            _feedback.OnExplosion(_model.ProjectileId, in point, _model.ExplosionRadius);
        }

        private void Complete(ProjectileEndReason reason)
        {
            if (!_model.IsFlying || _releaseRequested) return;
            _releaseRequested = true;

            EntityId projectileId = _model.ProjectileId;
            EntityId ownerId = _model.OwnerId;
            ProjectilePoolKey poolKey = _model.PoolKey;

            _model.Complete();
            _registry.Remove(this);
            _view.Deactivate();
            _events.Publish(new ProjectileCompletedEvent(projectileId, ownerId, reason));
            _pool.Release(poolKey, this);
        }
    }
}
