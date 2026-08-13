using System;
using UnityEngine;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Services;
using ZombieWar.Integration.GameState.Runtime;

namespace ZombieWar.Features.GameState.Unity.Runtime
{
    [DisallowMultipleComponent]
    public sealed class GameStateRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private GameStateSceneGateBinderReference sceneGateBinder;

        public IGameStateRuntime Runtime { get; private set; }
        public bool IsBound => Runtime != null;

        public void Bind(IGameStateRuntime runtime, IGameStateSceneGateBinding sceneGateBinding)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            if (sceneGateBinder != null) sceneGateBinder.Bind(sceneGateBinding);
        }

        public GameplayStateTransitionResult BeginGameplay() => Runtime != null ? Runtime.BeginGameplay() : default;
        public GameplayStateTransitionResult Pause() => Runtime != null ? Runtime.Pause() : default;
        public GameplayStateTransitionResult Resume() => Runtime != null ? Runtime.Resume() : default;
        public GameplayStateTransitionResult EnterGameOver() => Runtime != null ? Runtime.EnterGameOver() : default;
        public GameplayStateTransitionResult EnterLevelComplete() => Runtime != null ? Runtime.EnterLevelComplete() : default;
        public GameplayStateTransitionResult EnterEndGame() => Runtime != null ? Runtime.EnterEndGame() : default;
        public GameplayStateTransitionResult Deactivate() => Runtime != null ? Runtime.Deactivate() : default;

        private void OnDestroy()
        {
            if (sceneGateBinder != null) sceneGateBinder.Unbind();
            Runtime = null;
        }
    }

    // Small indirection keeps Feature Unity assembly independent from Zombie/Projectile Unity assemblies.
    public abstract class GameStateSceneGateBinderReference : MonoBehaviour
    {
        public abstract void Bind(IGameStateSceneGateBinding binding);
        public abstract void Unbind();
    }
}
