using System;
using UnityEngine;
using ZombieWar.Features.Control.Config;
using ZombieWar.Features.Control.Controller;
using ZombieWar.Features.Control.Input;
using ZombieWar.Features.Control.Model;
using ZombieWar.Features.Control.Ports;
using ZombieWar.Features.Control.View;

namespace ZombieWar.Composition
{
    /// <summary>
    /// Scene-owned composition adapter for the Control Feature.
    ///
    /// Responsibilities:
    /// - Convert authored ControlConfig into runtime JoystickSettings.
    /// - Construct the feature-owned DynamicJoystickModel and ControlController.
    /// - Connect the Unity DynamicJoystickView to the persistent gameplay input gate.
    /// - Route movement intent through the DI-provided IMovementIntentSink adapter.
    ///
    /// It does not move the Soldier Group directly and does not own gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ControlRuntimeRoot : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private ControlConfig controlConfig;

        [Header("View")]
        [SerializeField]
        private DynamicJoystickView controlView;

        private ControlController _controller;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public void Initialize(
            IGameplayInputState inputState,
            IMovementIntentSink movementIntentSink)
        {
            if (_isInitialized)
            {
                return;
            }

            ValidateReferences();

            if (inputState == null)
            {
                throw new ArgumentNullException(nameof(inputState));
            }

            if (movementIntentSink == null)
            {
                throw new ArgumentNullException(nameof(movementIntentSink));
            }

            var settings = controlConfig.CreateSettings();
            var model = new DynamicJoystickModel(settings);

            _controller = new ControlController(
                model,
                controlView,
                inputState,
                movementIntentSink);

            _isInitialized = true;
        }

        public void Shutdown()
        {
            if (!_isInitialized && _controller == null)
            {
                return;
            }

            _controller?.Dispose();
            _controller = null;
            _isInitialized = false;
        }

        private void ValidateReferences()
        {
            if (controlConfig == null)
            {
                throw new InvalidOperationException(
                    "ControlRuntimeRoot requires ControlConfig.");
            }

            if (controlView == null)
            {
                throw new InvalidOperationException(
                    "ControlRuntimeRoot requires DynamicJoystickView.");
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
