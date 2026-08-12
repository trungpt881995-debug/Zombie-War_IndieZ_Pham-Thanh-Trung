using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Factories;
using ZombieWar.Features.Weapon.Model;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Features.Weapon.Strategies;

namespace ZombieWar.Features.Weapon.Services
{
    public sealed class WeaponAttackService : IWeaponAttackService
    {
        private readonly IWeaponRuntime _runtime;
        private readonly IWeaponMuzzleProvider _muzzles;
        private readonly IWeaponFireStrategyProvider _strategies;
        private readonly IWeaponFireSessionFactory _sessions;
        private readonly Dictionary<EntityId, WeaponFireSessionModel> _byOwner =
            new Dictionary<EntityId, WeaponFireSessionModel>(4);

        public WeaponAttackService(
            IWeaponRuntime runtime,
            IWeaponMuzzleProvider muzzles,
            IWeaponFireStrategyProvider strategies,
            IWeaponFireSessionFactory sessions)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _muzzles = muzzles ?? throw new ArgumentNullException(nameof(muzzles));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        public void Update(EntityId ownerId, in WeaponTarget target, float deltaTime)
        {
            if (!_runtime.IsInitialized || !_runtime.GameplayEnabled)
            {
                ClearTarget(ownerId);
                return;
            }
            if (!_runtime.TryGetCurrentDefinition(out WeaponDefinition definition)) return;
            if (!_muzzles.TryGetMuzzle(ownerId, out WeaponMuzzle muzzle)) return;

            WeaponFireSessionModel session = GetOrCreate(ownerId);
            bool changed = session.HasTarget &&
                (session.Weapon != definition.Type || !session.TargetId.Equals(target.TargetId));
            if (changed)
            {
                EndCurrentStrategy(session);
                session.Clear();
            }

            var context = new WeaponFireContext(ownerId, in muzzle, in target);
            IWeaponFireStrategy strategy = _strategies.Get(definition.Type);

            if (!session.HasTarget)
            {
                session.Bind(definition.Type, target.TargetId);
                strategy.OnTargetAcquired(in definition, in context);
            }

            session.Tick(SanitizeDeltaTime(deltaTime));
            if (!session.Ready) return;

            strategy.Fire(in definition, in context);
            session.ConsumeCadence(definition.FireInterval);
        }

        public void ClearTarget(EntityId ownerId)
        {
            if (!_byOwner.TryGetValue(ownerId, out WeaponFireSessionModel session) || !session.HasTarget)
                return;
            EndCurrentStrategy(session);
            session.Clear();
        }

        public void ClearAll()
        {
            foreach (KeyValuePair<EntityId, WeaponFireSessionModel> pair in _byOwner)
            {
                WeaponFireSessionModel session = pair.Value;
                if (session.HasTarget) EndCurrentStrategy(session);
                session.Clear();
            }
        }

        private WeaponFireSessionModel GetOrCreate(EntityId ownerId)
        {
            if (_byOwner.TryGetValue(ownerId, out WeaponFireSessionModel session)) return session;
            session = _sessions.Create(ownerId);
            _byOwner.Add(ownerId, session);
            return session;
        }

        private void EndCurrentStrategy(WeaponFireSessionModel session)
        {
            if (!_runtime.TryGetDefinition(session.Weapon, out _)) return;
            _strategies.Get(session.Weapon).OnTargetCleared(session.OwnerId);
        }

        private static float SanitizeDeltaTime(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f) return 0f;
            return value;
        }
    }
}
