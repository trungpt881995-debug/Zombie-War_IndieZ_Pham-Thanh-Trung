using System;
using System.Collections.Generic;

namespace ZombieWar.Integration.GameState.Runtime
{
    public sealed class GameStateSceneGateRegistry : IGameStateSceneGateRegistry
    {
        private readonly List<IGameStateRuntimeGateTarget> _targets = new List<IGameStateRuntimeGateTarget>(4);
        public bool DesiredGameplayEnabled { get; private set; }
        public int Count => _targets.Count;

        public bool Bind(IGameStateRuntimeGateTarget target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (_targets.Contains(target)) return false;
            _targets.Add(target);
            target.SetGameplayEnabled(DesiredGameplayEnabled);
            return true;
        }

        public bool Unbind(IGameStateRuntimeGateTarget target)
        {
            if (target == null) return false;
            return _targets.Remove(target);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            DesiredGameplayEnabled = enabled;
            for (int i = 0; i < _targets.Count; i++)
                _targets[i].SetGameplayEnabled(enabled);
        }
    }
}
