using System;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Integration.GameState.Runtime;

namespace ZombieWar.Integration.GameState.Soldier
{
    public sealed class GameStateSoldierBinding : IGameStateSoldierBinding, IGameStateSoldierGate
    {
        private ISoldierGroupRuntime _runtime;
        private bool _desiredGameplayEnabled;

        public bool IsBound => _runtime != null;
        public EntityId GroupId => _runtime != null ? _runtime.GroupId : default;

        public void Bind(ISoldierGroupRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _runtime.SetGameplayEnabled(_desiredGameplayEnabled);
        }

        public void Unbind(ISoldierGroupRuntime runtime)
        {
            if (ReferenceEquals(_runtime, runtime)) _runtime = null;
        }

        public void SetGameplayEnabled(bool enabled)
        {
            _desiredGameplayEnabled = enabled;
            _runtime?.SetGameplayEnabled(enabled);
        }
    }
}
