using System;
using System.Collections.Generic;
using GameplayCore.Damage;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Collision;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Bootstrap
{
    /// <summary>
    /// Unity-side gameplay adapter for Flamethrower tick damage.
    /// Weapon core owns cadence/data; this adapter owns Physics overlap and Damage application.
    /// </summary>
    public sealed class WeaponFlameDamagePort : IWeaponFlamePort
    {
        private const int ColliderBufferSize = 512;
        private const float MinimumRange = 0.01f;
        private const float MinimumRadius = 0.01f;
        private const float MinimumDirectionSqrMagnitude = 0.000001f;

        private readonly IDamageService _damageService;
        private readonly Collider[] _colliderBuffer = new Collider[ColliderBufferSize];
        private readonly HashSet<GameplayEntityId> _uniqueEntities = new HashSet<GameplayEntityId>();

        public WeaponFlameDamagePort(IDamageService damageService)
        {
            _damageService = damageService ?? throw new ArgumentNullException(nameof(damageService));
        }

        public void Begin(in WeaponFlameRequest request)
        {
            // Flame VFX/audio lifetime is already handled by IWeaponFeedbackPort.
            // Gameplay damage is applied only from ApplyTick so Begin never deals a bonus hit.
        }

        public void ApplyTick(in WeaponFlameRequest request)
        {
            if (!TryBuildVolume(
                    in request,
                    out Vector3 start,
                    out Vector3 end,
                    out float radius))
            {
                return;
            }

            int count = Physics.OverlapCapsuleNonAlloc(
                start,
                end,
                radius,
                _colliderBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);

            _uniqueEntities.Clear();

            for (int i = 0; i < count; i++)
            {
                Collider collider = _colliderBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                IDamageable damageable = FindDamageable(collider);
                if (damageable == null ||
                    damageable.EntityId.Equals(request.OwnerId) ||
                    !_uniqueEntities.Add(damageable.EntityId))
                {
                    continue;
                }

                var damage = new DamageInfo(
                    request.OwnerId,
                    request.DamagePerTick,
                    "Flamethrower");

                _damageService.TryApply(damageable, damage);
            }
        }

        public void End(GameplayEntityId ownerId)
        {
            // No persistent gameplay state is owned here.
        }

        private static bool TryBuildVolume(
            in WeaponFlameRequest request,
            out Vector3 start,
            out Vector3 end,
            out float radius)
        {
            start = new Vector3(
                request.Origin.X,
                request.Origin.Y,
                request.Origin.Z);

            Vector3 direction = new Vector3(
                request.Direction.X,
                request.Direction.Y,
                request.Direction.Z);

            float sqrMagnitude = direction.sqrMagnitude;
            if (sqrMagnitude <= MinimumDirectionSqrMagnitude)
            {
                end = start;
                radius = 0f;
                return false;
            }

            direction /= Mathf.Sqrt(sqrMagnitude);

            float range = Mathf.Max(MinimumRange, request.Range);
            radius = Mathf.Max(MinimumRadius, request.Radius);
            end = start + direction * range;
            return true;
        }

        private static IDamageable FindDamageable(Collider collider)
        {
            ProjectileDamageableProxy proxy =
                collider.GetComponentInParent<ProjectileDamageableProxy>();

            return proxy != null
                ? proxy.Damageable
                : null;
        }
    }
}
