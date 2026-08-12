using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Integration.Zombie.Unity
{
    internal sealed class ZombiePoolReturnAdapter : IZombiePoolReturnPort
    {
        private readonly ZombiePool _pool;
        private readonly ZombieRuntimeHost _host;
        public ZombiePoolReturnAdapter(ZombiePool pool, ZombieRuntimeHost host) { _pool = pool; _host = host; }
        public void Return(EntityId entityId, ZombieReleaseReason reason) => _pool.Release(_host, entityId);
    }
}
