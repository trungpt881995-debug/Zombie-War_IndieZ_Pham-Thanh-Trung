using System;
using VContainer.Unity;
using ZombieWar.Integration.Audio.GameFlow;
using ZombieWar.Integration.Audio.GameState;
using ZombieWar.Integration.Audio.Level;
using ZombieWar.Integration.Audio.Soldier;

namespace ZombieWar.Bootstrap
{
    public sealed class AudioIntegrationRegistration :
        IStartable,
        IDisposable
    {
        private readonly GameStateAudioBridge _gameState;
        private readonly GameFlowMusicBridge _gameFlow;
        private readonly SoldierDamageAudioBridge _soldier;
        private readonly LevelAudioBridge _level;

        public AudioIntegrationRegistration(
            GameStateAudioBridge gameState,
            GameFlowMusicBridge gameFlow,
            SoldierDamageAudioBridge soldier,
            LevelAudioBridge level)
        {
            _gameState = gameState;
            _gameFlow = gameFlow;
            _soldier = soldier;
            _level = level;
        }

        public void Start()
        {
            _gameState.Start();
            _gameFlow.Start();
            _soldier.Start();
            _level.Start();
        }

        public void Dispose()
        {
            _level.Dispose();
            _soldier.Dispose();
            _gameFlow.Dispose();
            _gameState.Dispose();
        }
    }
}
