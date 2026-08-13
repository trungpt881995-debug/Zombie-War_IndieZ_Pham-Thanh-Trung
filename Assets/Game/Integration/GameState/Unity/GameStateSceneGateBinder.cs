using System;
using UnityEngine;
using ZombieWar.Features.GameState.Unity.Runtime;
using ZombieWar.Integration.GameState.Runtime;

namespace ZombieWar.Integration.GameState.Unity
{
    [DisallowMultipleComponent]
    public sealed class GameStateSceneGateBinder : GameStateSceneGateBinderReference
    {
        [SerializeField] private MonoBehaviour[] gateTargets = Array.Empty<MonoBehaviour>();
        private IGameStateSceneGateBinding _binding;

        public override void Bind(IGameStateSceneGateBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (_binding != null) Unbind();
            _binding = binding;
            for (int i = 0; i < gateTargets.Length; i++)
            {
                MonoBehaviour behaviour = gateTargets[i];
                if (behaviour == null) continue;
                if (!(behaviour is IGameStateRuntimeGateTarget target))
                    throw new InvalidOperationException($"GameState gate target at index {i} must implement IGameStateRuntimeGateTarget.");
                _binding.Bind(target);
            }
        }

        public override void Unbind()
        {
            if (_binding == null) return;
            for (int i = 0; i < gateTargets.Length; i++)
            {
                if (gateTargets[i] is IGameStateRuntimeGateTarget target)
                    _binding.Unbind(target);
            }
            _binding = null;
        }

        private void OnDestroy() => Unbind();
    }
}
