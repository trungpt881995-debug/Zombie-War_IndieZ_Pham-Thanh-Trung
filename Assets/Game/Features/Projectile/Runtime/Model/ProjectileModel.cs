using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Model
{
    public sealed class ProjectileModel
    {
        private readonly List<EntityId> _hitEntities = new List<EntityId>(8);

        public ProjectileState State { get; private set; } = ProjectileState.Inactive;
        public EntityId ProjectileId { get; private set; }
        public EntityId OwnerId { get; private set; }
        public ProjectilePoolKey PoolKey { get; private set; }
        public ProjectileImpactMode ImpactMode { get; private set; }
        public float Damage { get; private set; }
        public float MaxRange { get; private set; }
        public float MaxLifetime { get; private set; }
        public float ExplosionRadius { get; private set; }
        public float ElapsedTime { get; private set; }
        public float TravelledDistance { get; private set; }
        public ProjectilePoint LastPosition { get; private set; }
        public bool IsFlying => State == ProjectileState.Flying;

        public void Activate(EntityId projectileId, in ProjectileLaunchRequest request)
        {
            if (State == ProjectileState.Flying)
                throw new InvalidOperationException("Projectile is already flying.");

            ProjectileId = projectileId;
            OwnerId = request.OwnerId;
            PoolKey = request.PoolKey;
            ImpactMode = request.ImpactMode;
            Damage = request.Damage;
            MaxRange = request.MaxRange;
            MaxLifetime = request.MaxLifetime;
            ExplosionRadius = request.ExplosionRadius;
            ElapsedTime = 0f;
            TravelledDistance = 0f;
            LastPosition = request.Origin;
            _hitEntities.Clear();
            State = ProjectileState.Flying;
        }

        public void Advance(float deltaTime, in ProjectilePoint currentPosition)
        {
            if (!IsFlying) 
                return;

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            ElapsedTime += deltaTime;
            TravelledDistance += LastPosition.DistanceTo(in currentPosition);
            LastPosition = currentPosition;
        }

        public bool HasAlreadyHit(EntityId entityId)
        {
            for (int i = 0; i < _hitEntities.Count; i++)
            {
                if (_hitEntities[i] == entityId) 
                    return true;
            }
                
            return false;
        }

        public bool RegisterHit(EntityId entityId)
        {
            if (HasAlreadyHit(entityId)) return false;
            _hitEntities.Add(entityId);
            return true;
        }

        public bool HasReachedMaxRange => IsFlying && TravelledDistance >= MaxRange;
        public bool HasExpired => IsFlying && ElapsedTime >= MaxLifetime;

        public void Complete()
        {
            if (State == ProjectileState.Flying)
                State = ProjectileState.Completed;
        }

        public void Reset()
        {
            State = ProjectileState.Inactive;
            ProjectileId = default;
            OwnerId = default;
            PoolKey = default;
            ImpactMode = default;
            Damage = 0f;
            MaxRange = 0f;
            MaxLifetime = 0f;
            ExplosionRadius = 0f;
            ElapsedTime = 0f;
            TravelledDistance = 0f;
            LastPosition = default;
            _hitEntities.Clear();
        }
    }
}
