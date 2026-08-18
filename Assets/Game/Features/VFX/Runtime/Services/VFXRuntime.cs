using System;
using ZombieWar.Features.VFX.Catalog;
using ZombieWar.Features.VFX.Controller;
using ZombieWar.Features.VFX.Domain;
using ZombieWar.Features.VFX.Ports;

namespace ZombieWar.Features.VFX.Services
{
    public sealed class VFXRuntime : IVFXRuntime, IVFXRuntimeConfigurator
    {
        private readonly VFXController _controller;

        public VFXRuntime(VFXController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public event Action<VFXHandle> Completed
        {
            add => _controller.Completed += value;
            remove => _controller.Completed -= value;
        }

        public bool IsInitialized => _controller.IsInitialized;
        public VFXGameplayMode Mode => _controller.Mode;
        public int ActiveCount => _controller.ActiveCount;
        public VFXSnapshot Snapshot => _controller.Snapshot;

        public VFXHandle Play(in VFXRequest request) =>
            _controller.Play(in request);

        public bool Stop(VFXHandle handle) =>
            _controller.Stop(handle);

        public bool IsActive(VFXHandle handle) =>
            _controller.IsActive(handle);

        public void SetMode(VFXGameplayMode mode) =>
            _controller.SetMode(mode);

        public void Tick(float deltaTime) =>
            _controller.Tick(deltaTime);

        public void CancelAll() =>
            _controller.CancelAll();

        public void Initialize(
            IVFXCatalog catalog,
            IVFXPoolRegistry pools) =>
            _controller.Initialize(catalog, pools);

        public void Shutdown() =>
            _controller.Shutdown();
    }
}
