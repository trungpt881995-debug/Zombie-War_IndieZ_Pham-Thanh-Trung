using System;
using System.Collections.Generic;
using ZombieWar.Features.VFX.Catalog;
using ZombieWar.Features.VFX.Domain;
using ZombieWar.Features.VFX.Model;
using ZombieWar.Features.VFX.Ports;

namespace ZombieWar.Features.VFX.Controller
{
    public sealed class VFXController
    {
        private readonly VFXModel _model;
        private readonly List<VFXInstanceModel> _active =
            new List<VFXInstanceModel>(64);
        private readonly Dictionary<long, VFXInstanceModel> _byHandle =
            new Dictionary<long, VFXInstanceModel>(64);

        private IVFXCatalog _catalog;
        private IVFXPoolRegistry _pools;

        public VFXController(VFXModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Raised only when a timed OneShot reaches its configured Duration naturally.
        /// Manual Stop/CancelAll/Shutdown do not publish completion.
        /// </summary>
        public event Action<VFXHandle> Completed;

        public bool IsInitialized => _model.IsInitialized;
        public VFXGameplayMode Mode => _model.Mode;
        public int ActiveCount => _active.Count;

        public VFXSnapshot Snapshot =>
            new VFXSnapshot(
                IsInitialized,
                Mode,
                ActiveCount,
                _model.PlayedCount,
                _model.RejectedCount);

        public void Initialize(
            IVFXCatalog catalog,
            IVFXPoolRegistry pools)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (pools == null)
            {
                throw new ArgumentNullException(nameof(pools));
            }

            if (IsInitialized)
            {
                Shutdown();
            }

            _catalog = catalog;
            _pools = pools;
            _model.IsInitialized = true;
            ApplyPauseState();
        }

        public void Shutdown()
        {
            CancelAll();
            _pools?.ReleaseAll();
            _catalog = null;
            _pools = null;
            _model.IsInitialized = false;
        }

        public VFXHandle Play(in VFXRequest request)
        {
            if (!IsInitialized || !CanAccept(request.Id))
            {
                _model.RejectedCount++;
                return default;
            }

            if (!_catalog.TryGet(request.Id, out VFXDefinition definition))
            {
                _model.RejectedCount++;
                return default;
            }

            if (Mode == VFXGameplayMode.TerminalDrain &&
                (definition.Lifetime != VFXLifetimeMode.OneShot ||
                 !definition.AllowDuringTerminalDrain))
            {
                _model.RejectedCount++;
                return default;
            }

            if (!_pools.TryAcquire(request.Id, out IVFXLease lease) ||
                lease == null ||
                lease.View == null)
            {
                _model.RejectedCount++;
                return default;
            }

            long value = _model.NextHandle++;
            if (value <= 0)
            {
                _model.NextHandle = 2;
                value = 1;
            }

            var handle = new VFXHandle(value);
            var instance = new VFXInstanceModel
            {
                Handle = handle,
                Definition = definition,
                Lease = lease,
                Anchor = request.Anchor,
                Elapsed = 0f,
                Index = _active.Count,
                Paused = Mode == VFXGameplayMode.Suspended
            };

            VFXPose pose =
                request.Anchor != null && request.Anchor.IsValid
                    ? request.Anchor.Pose
                    : request.Pose;

            float scale =
                request.Scale > 0f
                    ? request.Scale
                    : definition.DefaultScale;

            lease.View.Activate(in pose, scale);
            lease.View.Play();
            lease.View.SetPaused(instance.Paused);

            _active.Add(instance);
            _byHandle.Add(value, instance);
            _model.PlayedCount++;

            return handle;
        }

        public bool Stop(VFXHandle handle)
        {
            if (!handle.IsValid ||
                !_byHandle.TryGetValue(handle.Value, out VFXInstanceModel instance))
            {
                return false;
            }

            Release(instance, true);
            return true;
        }

        public bool IsActive(VFXHandle handle)
        {
            return handle.IsValid && _byHandle.ContainsKey(handle.Value);
        }

        public void SetMode(VFXGameplayMode mode)
        {
            if (mode < VFXGameplayMode.Inactive ||
                mode > VFXGameplayMode.TerminalDrain)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            if (_model.Mode == mode)
            {
                return;
            }

            _model.Mode = mode;

            if (mode == VFXGameplayMode.Inactive)
            {
                CancelAll();
                return;
            }

            if (mode == VFXGameplayMode.TerminalDrain)
            {
                for (int i = _active.Count - 1; i >= 0; i--)
                {
                    if (_active[i].Definition.Lifetime == VFXLifetimeMode.Looping)
                    {
                        Release(_active[i], true);
                    }
                }
            }

            ApplyPauseState();
        }

        public void Tick(float deltaTime)
        {
            if (!IsInitialized ||
                Mode == VFXGameplayMode.Inactive ||
                Mode == VFXGameplayMode.Suspended)
            {
                return;
            }

            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                VFXInstanceModel instance = _active[i];

                if (instance.Anchor != null && instance.Anchor.IsValid)
                {
                    VFXPose pose = instance.Anchor.Pose;
                    instance.Lease.View.SetPose(in pose);
                }

                if (instance.Definition.Lifetime != VFXLifetimeMode.OneShot)
                {
                    continue;
                }

                instance.Elapsed += deltaTime;
                if (instance.Elapsed < instance.Definition.Duration)
                {
                    continue;
                }

                VFXHandle completedHandle = instance.Handle;
                Release(instance, false);

                // Publish only after the handle has been removed from the active set.
                // Subscribers may safely start the next VFX cycle from this callback.
                Completed?.Invoke(completedHandle);
            }
        }

        public void CancelAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Release(_active[i], true);
            }
        }

        private bool CanAccept(VFXId id)
        {
            _ = id;
            return Mode == VFXGameplayMode.Playing ||
                   Mode == VFXGameplayMode.TerminalDrain;
        }

        private void ApplyPauseState()
        {
            bool pause = _model.Mode == VFXGameplayMode.Suspended;

            for (int i = 0; i < _active.Count; i++)
            {
                VFXInstanceModel instance = _active[i];
                if (instance.Paused == pause)
                {
                    continue;
                }

                instance.Paused = pause;
                instance.Lease.View.SetPaused(pause);
            }
        }

        private void Release(VFXInstanceModel instance, bool stop)
        {
            if (instance == null || instance.Lease == null)
            {
                return;
            }

            if (stop)
            {
                instance.Lease.View.Stop();
            }

            instance.Lease.View.Deactivate();
            instance.Lease.Release();
            _byHandle.Remove(instance.Handle.Value);

            int index = instance.Index;
            int last = _active.Count - 1;

            if (index < 0 || index > last)
            {
                return;
            }

            if (index != last)
            {
                VFXInstanceModel moved = _active[last];
                _active[index] = moved;
                moved.Index = index;
            }

            _active.RemoveAt(last);
            instance.Index = -1;
        }
    }
}
