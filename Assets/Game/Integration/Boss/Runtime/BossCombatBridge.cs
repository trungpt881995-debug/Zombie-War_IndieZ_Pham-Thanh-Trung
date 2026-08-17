using System;
using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Boss.Controller;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Integration.Boss
{
    /// <summary>
    /// Cross-feature bridge exposed to Targeting and Damage.
    ///
    /// Gameplay/AI continues to own BossController.Position as the Boss root
    /// position. Targeting can optionally use a separate presentation aim
    /// point (normally the upper torso/chest) supplied by BossRuntimeHost.
    /// </summary>
    public sealed class BossCombatBridge : IDamageable, ITargetCandidate
    {
        private readonly BossController _controller;
        private readonly Func<BossPoint> _targetPointProvider;

        /// <summary>
        /// Backward-compatible constructor. Existing call sites continue to
        /// target BossController.Position.
        /// </summary>
        public BossCombatBridge(BossController controller)
            : this(controller, null)
        {
        }

        /// <summary>
        /// Preferred runtime constructor. The provider is evaluated whenever
        /// Targeting asks for Position, so a moving Boss always exposes the
        /// current AimPoint world position.
        /// </summary>
        public BossCombatBridge(
            BossController controller,
            Func<BossPoint> targetPointProvider)
        {
            _controller = controller
                ?? throw new ArgumentNullException(nameof(controller));

            _targetPointProvider = targetPointProvider;
        }

        public EntityId EntityId => _controller.EntityId;

        public bool IsAlive => _controller.IsAlive;

        public bool IsTargetable => _controller.IsTargetable;

        public TargetPoint Position
        {
            get
            {
                BossPoint p = _targetPointProvider != null
                    ? _targetPointProvider()
                    : _controller.Position;

                return new TargetPoint(
                    p.X,
                    p.Y,
                    p.Z);
            }
        }

        public void ApplyDamage(DamageInfo damage)
        {
            _controller.ReceiveDamage(damage);
        }
    }
}
