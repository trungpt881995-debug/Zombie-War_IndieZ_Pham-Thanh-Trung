using System;
using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Integration.Zombie
{
    public sealed class ZombieCombatBridge :
        IDamageable,
        ITargetCandidate
    {
        private readonly ZombieController _controller;
        private readonly Func<TargetPoint> _targetPointProvider;

        // Keep the current constructor for compatibility with any existing caller.
        public ZombieCombatBridge(ZombieController controller)
            : this(controller, null)
        {
        }

        public ZombieCombatBridge(
            ZombieController controller,
            Func<TargetPoint> targetPointProvider)
        {
            _controller = controller ??
                throw new ArgumentNullException(nameof(controller));

            _targetPointProvider = targetPointProvider;
        }

        public EntityId EntityId => _controller.EntityId;
        public bool IsAlive => _controller.IsAlive;
        public bool IsTargetable => _controller.IsTargetable;

        public TargetPoint Position
        {
            get
            {
                if (_targetPointProvider != null)
                    return _targetPointProvider();

                ZombiePoint p = _controller.Position;
                return new TargetPoint(p.X, p.Y, p.Z);
            }
        }

        public void ApplyDamage(DamageInfo damage) =>
            _controller.ReceiveDamage(damage);
    }
}
