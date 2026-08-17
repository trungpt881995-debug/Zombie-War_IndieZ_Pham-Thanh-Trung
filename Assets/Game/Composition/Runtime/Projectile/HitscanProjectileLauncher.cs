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
    /// Replaces physical Rigidbody projectiles with immediate Physics raycasts.
    ///
    /// StopOnHit:
    ///     first raycast hit stops the shot.
    ///
    /// Pierce:
    ///     damages each unique IDamageable in distance order until an environment
    ///     collider blocks the ray.
    ///
    /// ExplodeOnGround:
    ///     resolves an instant impact point and applies AoE damage there.
    ///
    /// Every shot draws one short-lived red LineRenderer tracer.
    /// </summary>
    public sealed class HitscanProjectileLauncher : IProjectileLauncher
    {
        private const float DefaultMuzzleForwardOffset = 0.05f;
        private const int ExplosionBufferSize = 128;

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
            _damageService = damageService ?? throw new ArgumentNullException(nameof(damageService));
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _tracers = tracers ?? throw new ArgumentNullException(nameof(tracers));
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
                    request.OwnerId,
                    request.PoolKey));

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
                        hit.point);

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

            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                direction,
                rayDistance,
                _hitMask,
                _triggerInteraction);

            if (hits != null && hits.Length > 0)
            {
                Array.Sort(hits, CompareHitsByDistance);
                _uniqueEntities.Clear();

                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];

                    IDamageable damageable =
                        FindDamageable(hit.collider);

                    if (damageable == null)
                    {
                        // Environment blocks the sniper ray.
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
                        hit.point);
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
                // Grenades used to be ballistic. In the new hitscan version,
                // the supplied target point is a better instant impact point
                // when the direct ray finds no surface.
                explosionPoint = ToVector3(request.TargetPoint);
            }

            _tracers.Show(origin, explosionPoint);

            float radius = Mathf.Max(0.01f, request.ExplosionRadius);

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

                ApplyDamage(
                    projectileId,
                    in request,
                    damageable,
                    explosionPoint);
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
            Vector3 worldPoint)
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

            _feedback.OnHit(
                projectileId,
                damageable.EntityId,
                in point);

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

        // Direct IDamageable component.
        if (behaviour is IDamageable damageable)
        {
            return damageable;
        }

        // Unity proxy that wraps a runtime IDamageable.
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

        private float GetRayDistance(
            float maxRange)
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

        private static int CompareHitsByDistance(
            RaycastHit left,
            RaycastHit right)
        {
            return left.distance.CompareTo(
                right.distance);
        }

        private static Vector3 ToVector3(
            in ProjectilePoint point)
        {
            return new Vector3(
                point.X,
                point.Y,
                point.Z);
        }
    }
}
