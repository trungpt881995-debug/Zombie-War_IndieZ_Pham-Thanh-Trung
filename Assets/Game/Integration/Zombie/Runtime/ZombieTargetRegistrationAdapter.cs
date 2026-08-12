using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Integration.Zombie
{
    public sealed class ZombieTargetRegistrationAdapter : IZombieTargetRegistrationPort
    {
        private readonly ITargetRegistry _registry;
        private ZombieCombatBridge _candidate;
        public ZombieTargetRegistrationAdapter(ITargetRegistry registry) => _registry = registry;
        public void Bind(ZombieCombatBridge candidate) => _candidate = candidate;
        public void Register(EntityId entityId) { if (_candidate != null) _registry.Register(_candidate); }
        public void Unregister(EntityId entityId) => _registry.Unregister(entityId);
    }
}
