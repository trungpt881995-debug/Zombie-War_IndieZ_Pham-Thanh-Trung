using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Catalog;
using ZombieWar.Features.Camera.Controller;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Model;
using ZombieWar.Features.Camera.Ports;

namespace ZombieWar.Features.Camera.Services
{
    public sealed class CameraRuntime : ICameraRuntime, ICameraRuntimeConfigurator
    {
        private readonly IEventBus _events;
        private CameraModel _model;
        private CameraController _controller;

        public bool IsInitialized => _controller != null;
        public CameraState State => IsInitialized ? _model.State : CameraState.Uninitialized;
        public bool GameplayEnabled => State == CameraState.Active;
        public bool HasTarget => IsInitialized && _model.HasTarget;
        public bool HasBounds => IsInitialized && _model.HasBounds;
        public CameraPoint RawTarget => IsInitialized ? _model.RawTarget : default;
        public CameraPoint ConstrainedTarget => IsInitialized ? _model.ConstrainedTarget : default;
        public CameraProfile Profile => IsInitialized ? _model.Profile : default;

        public CameraRuntime(IEventBus events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Initialize(
            in CameraProfile profile,
            ICameraShakeCatalog shakeCatalog,
            ICameraTargetProvider targetProvider,
            ICameraBoundsProvider boundsProvider,
            ICameraRig rig,
            ICameraShakeDriver shakeDriver)
        {
            if (IsInitialized) throw new InvalidOperationException("CameraRuntime is already initialized.");
            _model = new CameraModel();
            _controller = new CameraController(
                _model,
                targetProvider,
                boundsProvider,
                rig,
                shakeDriver ?? NullCameraShakeDriver.Instance,
                shakeCatalog ?? new CameraShakeCatalog(Array.Empty<CameraShakeDefinition>()),
                _events);
            _controller.Initialize(in profile);
        }

        public void Tick(float deltaTime)
        {
            if (!IsInitialized) return;
            _controller.Tick(SanitizeDeltaTime(deltaTime));
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (!IsInitialized) return;
            _controller.SetGameplayEnabled(enabled);
        }

        public bool SnapToTarget() => IsInitialized && _controller.SnapToTarget();

        public void ApplyProfile(in CameraProfile profile)
        {
            if (!IsInitialized) return;
            _controller.ApplyProfile(in profile);
        }

        public bool TryRequestShake(CameraShakeId shakeId) =>
            IsInitialized && _controller.TryRequestShake(shakeId);

        public void Shutdown()
        {
            if (!IsInitialized) return;
            _controller.Shutdown();
            _controller = null;
            _model = null;
        }

        private static float SanitizeDeltaTime(float value) =>
            float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? 0f : value;
    }
}
