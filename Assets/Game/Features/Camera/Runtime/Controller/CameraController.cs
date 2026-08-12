using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Catalog;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Events;
using ZombieWar.Features.Camera.Model;
using ZombieWar.Features.Camera.Ports;

namespace ZombieWar.Features.Camera.Controller
{
    public sealed class CameraController : IController
    {
        private readonly CameraModel _model;
        private readonly ICameraTargetProvider _targetProvider;
        private readonly ICameraBoundsProvider _boundsProvider;
        private readonly ICameraRig _rig;
        private readonly ICameraShakeDriver _shakeDriver;
        private readonly ICameraShakeCatalog _shakeCatalog;
        private readonly IEventBus _events;

        public CameraController(
            CameraModel model,
            ICameraTargetProvider targetProvider,
            ICameraBoundsProvider boundsProvider,
            ICameraRig rig,
            ICameraShakeDriver shakeDriver,
            ICameraShakeCatalog shakeCatalog,
            IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _targetProvider = targetProvider ?? throw new ArgumentNullException(nameof(targetProvider));
            _boundsProvider = boundsProvider ?? throw new ArgumentNullException(nameof(boundsProvider));
            _rig = rig ?? throw new ArgumentNullException(nameof(rig));
            _shakeDriver = shakeDriver ?? throw new ArgumentNullException(nameof(shakeDriver));
            _shakeCatalog = shakeCatalog ?? throw new ArgumentNullException(nameof(shakeCatalog));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Initialize(in CameraProfile profile)
        {
            if (!_rig.IsReady) throw new InvalidOperationException("Camera rig is not ready.");
            _model.Initialize(in profile);
            _rig.ApplyProfile(in profile);
            _rig.SetEnabled(false);
            _events.Publish(new CameraProfileChangedEvent(in profile));
            RefreshBounds();
        }

        public void Tick(float deltaTime)
        {
            if (_model.State != CameraState.Active) return;
            RefreshBounds();
            UpdateTarget(snap: false);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (_model.State == CameraState.Uninitialized) return;
            _model.SetGameplayEnabled(enabled);
            _rig.SetEnabled(enabled);
            if (!enabled) _shakeDriver.StopAll();
        }

        public bool SnapToTarget()
        {
            if (_model.State == CameraState.Uninitialized) return false;
            RefreshBounds();
            return UpdateTarget(snap: true);
        }

        public void ApplyProfile(in CameraProfile profile)
        {
            if (_model.State == CameraState.Uninitialized) return;
            if (_model.Profile.Equals(profile)) return;
            _model.SetProfile(in profile);
            _rig.ApplyProfile(in profile);
            _events.Publish(new CameraProfileChangedEvent(in profile));
        }

        public bool TryRequestShake(CameraShakeId shakeId)
        {
            if (_model.State != CameraState.Active) return false;
            if (!_shakeCatalog.TryGet(shakeId, out CameraShakeDefinition definition)) return false;
            var request = new CameraShakeRequest(in definition);
            return _shakeDriver.TryPlay(in request);
        }

        public void Shutdown()
        {
            _shakeDriver.StopAll();
            _rig.SetEnabled(false);
            _model.Reset();
        }

        private bool UpdateTarget(bool snap)
        {
            if (!_targetProvider.TryGetTarget(out CameraPoint raw))
            {
                _model.ClearTarget();
                return false;
            }

            bool hadTarget = _model.HasTarget;
            CameraPoint constrained = raw;
            if (_model.HasBounds)
            {
                CameraBounds bounds = _model.Bounds;
                constrained = bounds.Clamp(in raw);
            }

            _model.SetTarget(in raw, in constrained);
            if (!hadTarget) _events.Publish(new CameraTargetBoundEvent(in constrained));

            if (snap) _rig.SnapToTarget(in constrained);
            else _rig.SetTarget(in constrained);
            return true;
        }

        private void RefreshBounds()
        {
            bool hadBounds = _model.HasBounds;
            CameraBounds previous = _model.Bounds;

            if (_boundsProvider.TryGetBounds(out CameraBounds current) && current.IsValid)
            {
                if (!hadBounds || !previous.Equals(current))
                {
                    _model.SetBounds(in current);
                    _events.Publish(new CameraBoundsChangedEvent(true, in current));
                }
                return;
            }

            if (hadBounds)
            {
                _model.ClearBounds();
                CameraBounds none = default;
                _events.Publish(new CameraBoundsChangedEvent(false, in none));
            }
        }
    }
}
