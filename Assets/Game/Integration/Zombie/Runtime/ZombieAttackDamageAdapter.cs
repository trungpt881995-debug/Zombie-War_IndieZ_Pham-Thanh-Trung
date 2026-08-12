using GameplayCore.Damage;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Integration.Zombie
{
    public sealed class ZombieAttackDamageAdapter : IZombieAttackPort, IZombieAttackBinding
    {
        private readonly IDamageService _damageService;
        private IDamageable _sharedSoldierGroup;
        public ZombieAttackDamageAdapter(IDamageService damageService) => _damageService = damageService;
        public void BindSharedSoldierGroup(IDamageable damageable) => _sharedSoldierGroup = damageable;
        public void Unbind() => _sharedSoldierGroup = null;
        public bool TryAttack(in ZombieAttackRequest request)
        {
            if (_sharedSoldierGroup == null || !_sharedSoldierGroup.IsAlive || request.Damage <= 0f) return false;
            var damage = new DamageInfo(request.AttackerId, request.Damage, "ZombieAttack");
            return _damageService.TryApply(_sharedSoldierGroup, damage);
        }
    }
}
