using System;
using VContainer.Unity;
using ZombieWar.Integration.Score.Boss;
using ZombieWar.Integration.Score.Level;
using ZombieWar.Integration.Score.Zombie;

namespace ZombieWar.Bootstrap
{
    public sealed class ScoreIntegrationRegistration : IStartable, IDisposable
    {
        private readonly ZombieScoreBridge _zombie;
        private readonly BossScoreBridge _boss;
        private readonly LevelScoreLifecycleBridge _level;

        public ScoreIntegrationRegistration(ZombieScoreBridge zombie, BossScoreBridge boss, LevelScoreLifecycleBridge level)
        {
            _zombie = zombie;
            _boss = boss;
            _level = level;
        }

        public void Start()
        {
            _zombie.Start();
            _boss.Start();
            _level.Start();
        }

        public void Dispose()
        {
            _zombie.Dispose();
            _boss.Dispose();
            _level.Dispose();
        }
    }
}
