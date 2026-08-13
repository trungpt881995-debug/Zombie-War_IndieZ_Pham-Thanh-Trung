using System;
using VContainer.Unity;
using ZombieWar.Integration.Feedback.GameState;
using ZombieWar.Integration.Feedback.Level;
using ZombieWar.Integration.Feedback.Soldier;

namespace ZombieWar.Bootstrap
{
    public sealed class FeedbackIntegrationRegistration : IStartable, IDisposable
    {
        private readonly GameStateFeedbackBridge _gameState;
        private readonly SoldierDamageFeedbackBridge _soldier;
        private readonly LevelFeedbackBridge _level;

        public FeedbackIntegrationRegistration(
            GameStateFeedbackBridge gameState,
            SoldierDamageFeedbackBridge soldier,
            LevelFeedbackBridge level)
        {
            _gameState = gameState;
            _soldier = soldier;
            _level = level;
        }

        public void Start()
        {
            _gameState.Start();
            _soldier.Start();
            _level.Start();
        }

        public void Dispose()
        {
            _level.Dispose();
            _soldier.Dispose();
            _gameState.Dispose();
        }
    }
}
