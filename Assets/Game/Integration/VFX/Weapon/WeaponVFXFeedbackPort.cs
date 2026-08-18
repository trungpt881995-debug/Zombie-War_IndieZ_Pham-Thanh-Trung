using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.VFX.Domain;
using ZombieWar.Features.VFX.Ports;
using ZombieWar.Features.VFX.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Events;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Integration.VFX.Weapon
{
    /// <summary>
    /// Weapon-to-VFX presentation adapter.
    ///
    /// Flamethrower lifecycle rules:
    /// - Target loss / target change is a SOFT stop. The current visual cycle is
    ///   allowed to finish naturally. A newly acquired target may keep the same
    ///   cycle alive without restarting it.
    /// - Switching away from Flamethrower is a HARD stop. WeaponSelectedEvent is
    ///   the authoritative signal, so every active flame is removed immediately.
    /// </summary>
    public sealed class WeaponVFXFeedbackPort : IWeaponFeedbackPort, IDisposable
    {
        private readonly IVFXRuntime _vfx;
        private readonly IWeaponMuzzleProvider _muzzles;
        private readonly IDisposable _weaponSelectedSubscription;

        private readonly Dictionary<EntityId, FlameSession> _flames =
            new Dictionary<EntityId, FlameSession>(4);

        private readonly Dictionary<long, EntityId> _flameOwnerByHandle =
            new Dictionary<long, EntityId>(4);

        private bool _disposed;

        public WeaponVFXFeedbackPort(
            IVFXRuntime vfx,
            IWeaponMuzzleProvider muzzles,
            IEventSubscriber events)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _muzzles = muzzles ?? throw new ArgumentNullException(nameof(muzzles));

            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _vfx.Completed += OnVFXCompleted;
            _weaponSelectedSubscription =
                events.Subscribe<WeaponSelectedEvent>(OnWeaponSelected);
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            if (!TryPose(ownerId, out VFXPose pose))
            {
                return;
            }

            VFXId id = Map(weapon);
            if (id == VFXId.None)
            {
                return;
            }

            var request = new VFXRequest(id, in pose);
            _vfx.Play(in request);
        }

        public void OnFlameStarted(EntityId ownerId)
        {
            FlameSession session = GetOrCreateSession(ownerId);
            session.DesiredActive = true;

            EnsureFlameCycle(ownerId, session);
        }

        public void OnFlameStopped(EntityId ownerId)
        {
            if (!_flames.TryGetValue(ownerId, out FlameSession session))
            {
                return;
            }

            // IMPORTANT:
            // IWeaponFireStrategy.OnTargetCleared is also used during retargeting.
            // Therefore this callback must remain a SOFT stop. Do not infer a weapon
            // change here. WeaponSelectedEvent handles that case explicitly.
            session.DesiredActive = false;

            // If the active handle has already disappeared because of a global VFX
            // cancellation, clean the local session immediately.
            if (!session.Handle.IsValid || !_vfx.IsActive(session.Handle))
            {
                ForgetHandle(session);
                _flames.Remove(ownerId);
            }
        }

        private void OnWeaponSelected(WeaponSelectedEvent evt)
        {
            // The whole Soldier Group shares one selected weapon. The moment the
            // authoritative Weapon Feature switches away from Flamethrower, no
            // Soldier is allowed to keep a flame visual alive.
            if (evt.Previous == WeaponType.Flamethrower &&
                evt.Current != WeaponType.Flamethrower)
            {
                HardStopAllFlames();
            }
        }

        private void HardStopAllFlames()
        {
            if (_flames.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<EntityId, FlameSession> pair in _flames)
            {
                FlameSession session = pair.Value;
                if (session == null)
                {
                    continue;
                }

                session.DesiredActive = false;

                VFXHandle handle = session.Handle;
                if (handle.IsValid)
                {
                    _flameOwnerByHandle.Remove(handle.Value);

                    if (_vfx.IsActive(handle))
                    {
                        _vfx.Stop(handle);
                    }
                }

                session.Handle = default;
            }

            _flames.Clear();
            _flameOwnerByHandle.Clear();
        }

        private FlameSession GetOrCreateSession(EntityId ownerId)
        {
            if (_flames.TryGetValue(ownerId, out FlameSession session))
            {
                return session;
            }

            session = new FlameSession(
                new MuzzleAnchor(ownerId, _muzzles));

            _flames.Add(ownerId, session);
            return session;
        }

        private void EnsureFlameCycle(
            EntityId ownerId,
            FlameSession session)
        {
            if (session == null || !session.DesiredActive)
            {
                return;
            }

            if (session.Handle.IsValid && _vfx.IsActive(session.Handle))
            {
                return;
            }

            ForgetHandle(session);

            if (!TryPose(ownerId, out VFXPose pose))
            {
                return;
            }

            var request = new VFXRequest(
                VFXId.FlamethrowerLoop,
                in pose,
                0f,
                session.Anchor);

            VFXHandle handle = _vfx.Play(in request);
            if (!handle.IsValid)
            {
                return;
            }

            session.Handle = handle;
            _flameOwnerByHandle[handle.Value] = ownerId;
        }

        private void OnVFXCompleted(VFXHandle handle)
        {
            if (!handle.IsValid ||
                !_flameOwnerByHandle.TryGetValue(
                    handle.Value,
                    out EntityId ownerId))
            {
                return;
            }

            _flameOwnerByHandle.Remove(handle.Value);

            if (!_flames.TryGetValue(ownerId, out FlameSession session) ||
                session.Handle != handle)
            {
                return;
            }

            session.Handle = default;

            if (!session.DesiredActive)
            {
                _flames.Remove(ownerId);
                return;
            }

            // DesiredActive can only remain true while Flamethrower target flow is
            // active. A real weapon switch has already removed this session through
            // OnWeaponSelected(), so a completed old cycle cannot resurrect flame.
            EnsureFlameCycle(ownerId, session);
        }

        private void ForgetHandle(FlameSession session)
        {
            if (session == null || !session.Handle.IsValid)
            {
                return;
            }

            _flameOwnerByHandle.Remove(session.Handle.Value);
            session.Handle = default;
        }

        private bool TryPose(
            EntityId ownerId,
            out VFXPose pose)
        {
            if (!_muzzles.TryGetMuzzle(ownerId, out WeaponMuzzle muzzle))
            {
                pose = default;
                return false;
            }

            var point = new VFXPoint(
                muzzle.Position.X,
                muzzle.Position.Y,
                muzzle.Position.Z);

            var direction = new VFXDirection(
                muzzle.Forward.X,
                muzzle.Forward.Y,
                muzzle.Forward.Z);

            pose = new VFXPose(in point, in direction);
            return true;
        }

        private static VFXId Map(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Pistol:
                    return VFXId.PistolMuzzle;

                case WeaponType.AK:
                    return VFXId.AKMuzzle;

                case WeaponType.Shotgun:
                    return VFXId.ShotgunMuzzle;

                case WeaponType.SniperRifle:
                    return VFXId.SniperMuzzle;

                case WeaponType.GrenadeLauncher:
                    return VFXId.GrenadeMuzzle;

                default:
                    return VFXId.None;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _weaponSelectedSubscription?.Dispose();
            _vfx.Completed -= OnVFXCompleted;

            HardStopAllFlames();
        }

        private sealed class FlameSession
        {
            public FlameSession(MuzzleAnchor anchor)
            {
                Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            }

            public bool DesiredActive;
            public VFXHandle Handle;
            public MuzzleAnchor Anchor { get; }
        }

        private sealed class MuzzleAnchor : IVFXAnchor
        {
            private readonly EntityId _ownerId;
            private readonly IWeaponMuzzleProvider _muzzles;

            public MuzzleAnchor(
                EntityId ownerId,
                IWeaponMuzzleProvider muzzles)
            {
                _ownerId = ownerId;
                _muzzles = muzzles ??
                    throw new ArgumentNullException(nameof(muzzles));
            }

            public bool IsValid =>
                _muzzles.TryGetMuzzle(_ownerId, out _);

            public VFXPose Pose
            {
                get
                {
                    _muzzles.TryGetMuzzle(
                        _ownerId,
                        out WeaponMuzzle muzzle);

                    var point = new VFXPoint(
                        muzzle.Position.X,
                        muzzle.Position.Y,
                        muzzle.Position.Z);

                    var direction = new VFXDirection(
                        muzzle.Forward.X,
                        muzzle.Forward.Y,
                        muzzle.Forward.Z);

                    return new VFXPose(in point, in direction);
                }
            }
        }
    }
}
