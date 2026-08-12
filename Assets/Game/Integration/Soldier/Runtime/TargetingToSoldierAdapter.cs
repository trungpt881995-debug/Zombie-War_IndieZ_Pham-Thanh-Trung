using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Factories;

namespace ZombieWar.Integration.Soldier
{
    /// <summary>
    /// One Targeting session is lazily created per Soldier EntityId.
    /// The adapter owns cross-feature DTO conversion only.
    /// </summary>
    public sealed class TargetingToSoldierAdapter :
        ISoldierTargetingPort,
        IDisposable
    {
        private readonly ITargetingFactory _factory;

        private readonly Dictionary<
            EntityId,
            ITargetingSession> _sessions =
                new Dictionary<
                    EntityId,
                    ITargetingSession>();

        public TargetingToSoldierAdapter(
            ITargetingFactory factory)
        {
            _factory = factory ??
                throw new ArgumentNullException(nameof(factory));
        }

        public SoldierTargetInfo Evaluate(
            EntityId soldierId,
            in SoldierPoint position,
            float targetRange)
        {
            ITargetingSession session =
                GetOrCreate(
                    soldierId);

            var origin =
                new TargetPoint(
                    position.X,
                    position.Y,
                    position.Z);

            var context =
                new TargetingContext(
                    in origin,
                    SanitizeRange(targetRange));

            TargetingResult result =
                session.Evaluate(
                    in context);

            if (!result.HasTarget)
                return SoldierTargetInfo.None;

            var targetPosition =
                new SoldierPoint(
                    result.TargetPosition.X,
                    result.TargetPosition.Y,
                    result.TargetPosition.Z);

            return SoldierTargetInfo.From(
                result.TargetId,
                in targetPosition);
        }

        public void Clear(
            EntityId soldierId)
        {
            if (_sessions.TryGetValue(
                soldierId,
                out ITargetingSession session))
            {
                session.Clear(
                    TargetLossReason.ManualClear);
            }
        }

        public void Dispose()
        {
            foreach (
                KeyValuePair<
                    EntityId,
                    ITargetingSession> pair
                in _sessions)
            {
                pair.Value.Clear(
                    TargetLossReason.ManualClear);
            }

            _sessions.Clear();
        }

        private ITargetingSession GetOrCreate(
            EntityId soldierId)
        {
            if (_sessions.TryGetValue(
                soldierId,
                out ITargetingSession session))
            {
                return session;
            }

            session =
                _factory.Create(
                    soldierId);

            _sessions.Add(
                soldierId,
                session);

            return session;
        }

        private static float SanitizeRange(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value < 0f)
            {
                return 0f;
            }

            return value;
        }
    }
}
