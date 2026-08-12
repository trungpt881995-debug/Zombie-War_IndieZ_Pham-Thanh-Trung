using System;

namespace ZombieWar.Features.Control.Input
{
    public sealed class GameplayInputGate : IGameplayInputState
    {
        private bool _gameplayInputEnabled;

        public bool GameplayInputEnabled => _gameplayInputEnabled;

        public event Action<bool> GameplayInputEnabledChanged;

        public GameplayInputGate(bool gameplayInputEnabled = true)
        {
            _gameplayInputEnabled = gameplayInputEnabled;
        }

        public void SetGameplayInputEnabled(bool enabled)
        {
            if (_gameplayInputEnabled == enabled)
                return;

            _gameplayInputEnabled = enabled;
            GameplayInputEnabledChanged?.Invoke(enabled);
        }
    }
}
