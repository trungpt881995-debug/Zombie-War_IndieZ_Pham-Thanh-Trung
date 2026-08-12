using System;
using UnityEngine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Ports;
using ZombieWar.Features.Camera.Services;
using ZombieWar.Features.Camera.Unity.Config;

namespace ZombieWar.Features.Camera.Unity.Runtime
{
    public sealed class CameraRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private CameraConfig config;
        [SerializeField] private CameraTargetProviderBehaviour targetProvider;
        [SerializeField] private CameraRigBehaviour cameraRig;
        [SerializeField] private CameraShakeDriverBehaviour shakeDriver;

        private ICameraRuntime _runtime;
        private ICameraRuntimeConfigurator _configurator;

        public bool IsInitialized => _runtime != null && _runtime.IsInitialized;
        public ICameraRuntime Runtime => _runtime;

        public void Initialize(
            ICameraRuntime runtime,
            ICameraRuntimeConfigurator configurator,
            ICameraBoundsProvider boundsProvider)
        {
            if (IsInitialized) return;
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            if (boundsProvider == null) throw new ArgumentNullException(nameof(boundsProvider));
            if (config == null) throw new InvalidOperationException("CameraConfig is not assigned.");
            if (targetProvider == null) throw new InvalidOperationException("Camera target provider is not assigned.");
            if (cameraRig == null) throw new InvalidOperationException("Camera rig is not assigned.");
            if (!cameraRig.IsReady) throw new InvalidOperationException("Camera rig is not ready.");

            CameraProfile profile = config.BuildProfile();
            ICameraShakeDriver runtimeShake = shakeDriver != null ? shakeDriver : NullCameraShakeDriver.Instance;

            configurator.Initialize(
                in profile,
                config.BuildShakeCatalog(),
                targetProvider,
                boundsProvider,
                cameraRig,
                runtimeShake);

            _runtime = runtime;
            _configurator = configurator;
            _runtime.SetGameplayEnabled(config.StartGameplayEnabled);
            if (config.SnapOnInitialize) _runtime.SnapToTarget();
        }

        public void SetGameplayEnabled(bool enabled) => _runtime?.SetGameplayEnabled(enabled);
        public bool SnapToTarget() => _runtime != null && _runtime.SnapToTarget();
        public bool RequestShake(CameraShakeId id) => _runtime != null && _runtime.TryRequestShake(id);

        private void LateUpdate()
        {
            if (_runtime == null || !_runtime.IsInitialized) return;
            _runtime.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_configurator != null) _configurator.Shutdown();
            _runtime = null;
            _configurator = null;
        }
    }
}
