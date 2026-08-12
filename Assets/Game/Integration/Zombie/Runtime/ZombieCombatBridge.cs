using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Integration.Zombie
{
    public sealed class ZombieCombatBridge : IDamageable, ITargetCandidate
    {
        private readonly ZombieController _controller;
        public ZombieCombatBridge(ZombieController controller) => _controller = controller;
        public EntityId EntityId => _controller.EntityId;
        public bool IsAlive => _controller.IsAlive;
        public bool IsTargetable => _controller.IsTargetable;
        public TargetPoint Position
        {
            get
            {
                ZombiePoint p = _controller.Position;
                return new TargetPoint(p.X, p.Y, p.Z);
            }
        }
        public void ApplyDamage(DamageInfo damage) => _controller.ReceiveDamage(damage);
    }
}
