using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using EntityId = GameplayCore.Entities.EntityId;
using UnityEngine;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Events;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Services;
using ZombieWar.Features.Projectile.Unity.Collision;

namespace ZombieWar.Composition.Projectile
{
    /// <summary>
    /// Immediate Physics-based projectile resolution.
    /// No Rigidbody projectile, flight simulation, collision relay or projectile pool is used.
    /// </summary>
    public sealed class HitscanProjectileLauncher : IProjectileLauncher
    {
        private const float DefaultMuzzleForwardOffset = 0.05f;
        private const int RaycastBufferSize = 512;
        private const int ExplosionBufferSize = 512;

        private readonly IEntityIdGenerator _ids;
        private readonly IDamageService _damageService;
        private readonly IProjectileFeedbackPort _feedback;
        private readonly IEventBus _events;
        private readonly HitscanTracerPool _tracers;
        private readonly int _hitMask;
        private readonly QueryTriggerInteraction _triggerInteraction;
        private readonly float _muzzleForwardOffset;

        private readonly HashSet<EntityId> _uniqueEntities =
            new HashSet<EntityId>();

        private readonly List<MonoBehaviour> _behaviourBuffer =
            new List<MonoBehaviour>(8);

        private readonly RaycastHit[] _raycastBuffer =
            new RaycastHit[RaycastBufferSize];

        private readonly Collider[] _explosionBuffer =
            new Collider[ExplosionBufferSize];

        public HitscanProjectileLauncher(
            IEntityIdGenerator ids,
            IDamageService damageService,
            IProjectileFeedbackPort feedback,
            IEventBus events,
            HitscanTracerPool tracers,
            int hitMask = ~0,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore,
            float muzzleForwardOffset = DefaultMuzzleForwardOffset)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
            _damageService = damageService ??
                throw new ArgumentNullException(nameof(damageService));
            _feedback = feedback ??
                throw new ArgumentNullException(nameof(feedback));
            _events = events ??
                throw new ArgumentNullException(nameof(events));
            _tracers = tracers ??
                throw new ArgumentNullException(nameof(tracers));
            _hitMask = hitMask;
            _triggerInteraction = triggerInteraction;
            _muzzleForwardOffset = Mathf.Max(0f, muzzleForwardOffset);
        }

        public bool TryLaunch(
            in ProjectileLaunchRequest request,
            out EntityId projectileId)
        {
            projectileId = _ids.Next();

            Vector3 origin = ToVector3(request.Origin);
            Vector3 direction = new Vector3(
                request.Direction.X,
                request.Direction.Y,
                request.Direction.Z);

            if (direction.sqrMagnitude <= 0.000001f)
            {
                projectileId = default;
                return false;
            }

            direction.Normalize();

            _events.Publish(
                new ProjectileLaunchedEvent(
                    projectileId,
                    request.OwnerId));

            switch (request.ImpactMode)
            {
                case ProjectileImpactMode.Pierce:
                    FirePiercing(
                        projectileId,
                        in request,
                        origin,
                        direction);
                    break;

                case ProjectileImpactMode.ExplodeOnGround:
                    FireExplosion(
                        projectileId,
                        in request,
                        origin,
                        direction);
                    break;

                case ProjectileImpactMode.StopOnHit:
                default:
                    FireSingle(
                        projectileId,
                        in request,
                        origin,
                        direction);
                    break;
            }

            return true;
        }

        private void FireSingle(
            EntityId projectileId,
            in ProjectileLaunchRequest request,
            Vector3 origin,
            Vector3 direction)
        {
            Vector3 rayOrigin = GetRayOrigin(origin, direction);
            float rayDistance = GetRayDistance(request.MaxRange);

            Vector3 endPoint =
                origin + direction * request.MaxRange;

            ProjectileEndReason endReason =
                ProjectileEndReason.MaxRangeReached;

            if (Physics.Raycast(
                    rayOrigin,
                    direction,
                    out RaycastHit hit,
                    rayDistance,
                    _hitMask,
                    _triggerInteraction))
            {
                endPoint = hit.point;

                IDamageable damageable =
                    FindDamageable(hit.collider);

                if (damageable != null)
                {
                    ApplyDamage(
                        projectileId,
                        in request,
                        damageable,
                        hit.point,
                        playHitFeedback: true);

                    endReason = ProjectileEndReason.Hit;
                }
                else
                {
                    endReason = ProjectileEndReason.EnvironmentHit;
                }
            }

            _tracers.Show(origin, endPoint);
            PublishCompleted(projectileId, request.OwnerId, endReason);
        }

        private void FirePiercing(
            EntityId projectileId,
            in ProjectileLaunchRequest request,
            Vector3 origin,
            Vector3 direction)
        {
            Vector3 rayOrigin = GetRayOrigin(origin, direction);
            float rayDistance = GetRayDistance(request.MaxRange);

            Vector3 endPoint =
                origin + direction * request.MaxRange;

            ProjectileEndReason endReason =
                ProjectileEndReason.MaxRangeReached;

            int hitCount = Physics.RaycastNonAlloc(
                rayOrigin,
                direction,
                _raycastBuffer,
                rayDistance,
                _hitMask,
                _triggerInteraction);

            if (hitCount > 0)
            {
                Array.Sort(
                    _raycastBuffer,
                    0,
                    hitCount,
                    RaycastHitDistanceComparer.Instance);

                _uniqueEntities.Clear();

                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = _raycastBuffer[i];

                    IDamageable damageable =
                        FindDamageable(hit.collider);

                    if (damageable == null)
                    {
                        endPoint = hit.point;
                        endReason = ProjectileEndReason.EnvironmentHit;
                        break;
                    }

                    if (!_uniqueEntities.Add(damageable.EntityId))
                    {
                        continue;
                    }

                    ApplyDamage(
                        projectileId,
                        in request,
                        damageable,
                        hit.point,
                        playHitFeedback: true);
                }
            }

            _tracers.Show(origin, endPoint);
            PublishCompleted(projectileId, request.OwnerId, endReason);
        }

        private void FireExplosion(
            EntityId projectileId,
            in ProjectileLaunchRequest request,
            Vector3 origin,
            Vector3 direction)
        {
            Vector3 rayOrigin = GetRayOrigin(origin, direction);
            float rayDistance = GetRayDistance(request.MaxRange);

            Vector3 explosionPoint =
                origin + direction * request.MaxRange;

            if (Physics.Raycast(
                    rayOrigin,
                    direction,
                    out RaycastHit hit,
                    rayDistance,
                    _hitMask,
                    _triggerInteraction))
            {
                explosionPoint = hit.point;
            }
            else if (request.HasTargetPoint)
            {
                explosionPoint = ToVector3(request.TargetPoint);
            }

            _tracers.Show(origin, explosionPoint);

            float radius = Mathf.Max(
                0.01f,
                request.ExplosionRadius);

            int count = Physics.OverlapSphereNonAlloc(
                explosionPoint,
                radius,
                _explosionBuffer,
                _hitMask,
                _triggerInteraction);

            _uniqueEntities.Clear();

            for (int i = 0; i < count; i++)
            {
                Collider collider = _explosionBuffer[i];

                if (collider == null)
                {
                    continue;
                }

                IDamageable damageable =
                    FindDamageable(collider);

                if (damageable == null ||
                    !_uniqueEntities.Add(damageable.EntityId))
                {
                    continue;
                }

                // One grenade can damage a large horde. Do not emit BulletImpact/BloodImpact
                // presentation for every affected Zombie; emit the gameplay hit fact for each,
                // then one Explosion presentation below.
                ApplyDamage(
                    projectileId,
                    in request,
                    damageable,
                    explosionPoint,
                    playHitFeedback: false);
            }

            var point = new ProjectilePoint(
                explosionPoint.x,
                explosionPoint.y,
                explosionPoint.z);

            _feedback.OnExplosion(
                projectileId,
                in point,
                radius);

            PublishCompleted(
                projectileId,
                request.OwnerId,
                ProjectileEndReason.GroundExplosion);
        }

        private void ApplyDamage(
            EntityId projectileId,
            in ProjectileLaunchRequest request,
            IDamageable damageable,
            Vector3 worldPoint,
            bool playHitFeedback)
        {
            var info = new DamageInfo(
                request.OwnerId,
                request.Damage,
                "HitscanProjectile");

            if (!_damageService.TryApply(damageable, info))
            {
                return;
            }

            var point = new ProjectilePoint(
                worldPoint.x,
                worldPoint.y,
                worldPoint.z);

            if (playHitFeedback)
            {
                _feedback.OnHit(
                    projectileId,
                    damageable.EntityId,
                    in point);
            }

            _events.Publish(
                new ProjectileHitEvent(
                    projectileId,
                    request.OwnerId,
                    damageable.EntityId,
                    request.Damage));
        }

        private IDamageable FindDamageable(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            _behaviourBuffer.Clear();

            collider.GetComponentsInParent(
                true,
                _behaviourBuffer);

            for (int i = 0; i < _behaviourBuffer.Count; i++)
            {
                MonoBehaviour behaviour = _behaviourBuffer[i];

                if (behaviour is IDamageable damageable)
                {
                    return damageable;
                }

                if (behaviour is ProjectileDamageableProxy proxy)
                {
                    IDamageable proxyDamageable = proxy.Damageable;

                    if (proxyDamageable != null)
                    {
                        return proxyDamageable;
                    }
                }
            }

            return null;
        }

        private Vector3 GetRayOrigin(
            Vector3 origin,
            Vector3 direction)
        {
            return origin +
                   direction * _muzzleForwardOffset;
        }

        private float GetRayDistance(float maxRange)
        {
            return Mathf.Max(
                0.001f,
                maxRange - _muzzleForwardOffset);
        }

        private void PublishCompleted(
            EntityId projectileId,
            EntityId ownerId,
            ProjectileEndReason reason)
        {
            _events.Publish(
                new ProjectileCompletedEvent(
                    projectileId,
                    ownerId,
                    reason));
        }

        private static Vector3 ToVector3(in ProjectilePoint point)
        {
            return new Vector3(
                point.X,
                point.Y,
                point.Z);
        }

        private sealed class RaycastHitDistanceComparer :
            IComparer<RaycastHit>
        {
            public static readonly RaycastHitDistanceComparer Instance =
                new RaycastHitDistanceComparer();

            private RaycastHitDistanceComparer()
            {
            }

            public int Compare(RaycastHit left, RaycastHit right)
            {
                return left.distance.CompareTo(right.distance);
            }
        }
    }
}
