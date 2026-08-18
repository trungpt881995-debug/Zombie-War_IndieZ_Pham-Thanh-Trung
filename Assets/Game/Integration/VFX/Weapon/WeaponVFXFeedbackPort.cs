using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.VFX.Domain;
using ZombieWar.Features.VFX.Ports;
using ZombieWar.Features.VFX.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Integration.VFX.Weapon
{
    /// <summary>
    /// Weapon-to-VFX presentation adapter.
    ///
    /// Flamethrower target ownership and visual lifetime are intentionally decoupled:
    /// losing/changing a target no longer hard-stops an already-playing flame cycle.
    /// The current OneShot cycle is allowed to finish naturally. If the weapon still
    /// wants flame presentation when that cycle completes, the next cycle is started.
    /// </summary>
    public sealed class WeaponVFXFeedbackPort : IWeaponFeedbackPort
    {
        private readonly IVFXRuntime _vfx;
        private readonly IWeaponMuzzleProvider _muzzles;

        private readonly Dictionary<EntityId, FlameSession> _flames =
            new Dictionary<EntityId, FlameSession>(4);

        private readonly Dictionary<long, EntityId> _flameOwnerByHandle =
            new Dictionary<long, EntityId>(4);

        public WeaponVFXFeedbackPort(
            IVFXRuntime vfx,
            IWeaponMuzzleProvider muzzles)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _muzzles = muzzles ?? throw new ArgumentNullException(nameof(muzzles));

            _vfx.Completed += OnVFXCompleted;
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

            // Important: target loss/retarget must NOT interrupt the current visual.
            // We only prevent another cycle from starting after this one completes.
            session.DesiredActive = false;

            // A stale handle can exist after a global VFX cancellation. Clean it up
            // immediately, but never Stop() an active flame from this callback.
            if (!session.Handle.IsValid || !_vfx.IsActive(session.Handle))
            {
                ForgetHandle(session);
                _flames.Remove(ownerId);
            }
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

            // Still targeting/firing after the previous cycle completed:
            // immediately start the next visual cycle from the current muzzle pose.
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
