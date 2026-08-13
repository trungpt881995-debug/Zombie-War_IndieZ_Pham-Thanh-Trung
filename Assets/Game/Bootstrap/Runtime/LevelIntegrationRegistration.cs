using System; using VContainer.Unity; using ZombieWar.Integration.Level.Zombie; using ZombieWar.Integration.Level.Spawn; using ZombieWar.Integration.Level.Soldier;
namespace ZombieWar.Bootstrap
{
    public sealed class LevelIntegrationRegistration:IStartable,IDisposable
    {
        private readonly ZombieKillToLevelProgressAdapter _zombie; private readonly LevelSpawnProgressionBridge _spawn; private readonly LevelSoldierProgressionBridge _soldier;
        public LevelIntegrationRegistration(ZombieKillToLevelProgressAdapter zombie,LevelSpawnProgressionBridge spawn,LevelSoldierProgressionBridge soldier){_zombie=zombie;_spawn=spawn;_soldier=soldier;}
        public void Start(){_zombie.Start();_spawn.Start();_soldier.Start();}
        public void Dispose(){_zombie.Dispose();_spawn.Dispose();_soldier.Dispose();}
    }
}
