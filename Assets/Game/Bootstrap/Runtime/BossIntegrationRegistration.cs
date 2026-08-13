using System;
using VContainer.Unity;
using ZombieWar.Integration.Boss.Level;

namespace ZombieWar.Bootstrap
{
    public sealed class BossIntegrationRegistration : IStartable, IDisposable
    {
        private readonly LevelBossPhaseBridge _phase;
        private readonly BossDeathToLevelBridge _death;

        public BossIntegrationRegistration(
            LevelBossPhaseBridge phase,
            BossDeathToLevelBridge death)
        {
            _phase = phase;
            _death = death;
        }

        public void Start()
        {
            _phase.Start();
            _death.Start();
        }

        public void Dispose()
        {
            _phase.Dispose();
            _death.Dispose();
        }
    }
}
