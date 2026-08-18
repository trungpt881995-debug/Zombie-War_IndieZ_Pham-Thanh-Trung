using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.VFX.Domain;
using ZombieWar.Features.VFX.Ports;
using ZombieWar.Features.VFX.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Features.Weapon.Services;

namespace ZombieWar.Integration.VFX.Weapon
{
    /// <summary>
    /// Weapon-to-VFX presentation adapter.
    ///
    /// Flamethrower semantics:
    /// - Losing/changing only the Zombie target is a soft stop. The active visual
    ///   cycle is allowed to finish naturally and is not restarted during retarget.
    /// - Switching away from Flamethrower (or disabling gameplay) is a hard stop.
    ///   The active flame is stopped immediately because the owner is no longer
    ///   using a flamethrower.
    /// </summary>
    public sealed class WeaponVFXFeedbackPort : IWeaponFeedbackPort
    {
        private readonly IVFXRuntime _vfx;
        private readonly IWeaponMuzzleProvider _muzzles;
        private readonly IWeaponRuntime _weaponRuntime;

        private readonly Dictionary<EntityId, FlameSession> _flames =
            new Dictionary<EntityId, FlameSession>(4);

        private readonly Dictionary<long, EntityId> _flameOwnerByHandle =
            new Dictionary<long, EntityId>(4);

        public WeaponVFXFeedbackPort(
            IVFXRuntime vfx,
            IWeaponMuzzleProvider muzzles,
            IWeaponRuntime weaponRuntime)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _muzzles = muzzles ?? throw new ArgumentNullException(nameof(muzzles));
            _weaponRuntime = weaponRuntime ??
                throw new ArgumentNullException(nameof(weaponRuntime));

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

            session.DesiredActive = false;

            // WeaponAttackService uses the same OnTargetCleared callback for both
            // retargeting and weapon changes. Distinguish them using the authoritative
            // Weapon runtime state:
            //
            // Flamethrower still selected -> target-only clear -> let visual drain.
            // Other weapon selected / gameplay disabled -> immediately remove flame.
            if (ShouldHardStopFlame())
            {
                HardStop(ownerId, session);
                return;
            }

            // Target-only loss/retarget: do NOT stop an active cycle. If the handle
            // is already stale (for example after a global VFX cancellation), clean
            // the presentation session immediately.
            if (!session.Handle.IsValid || !_vfx.IsActive(session.Handle))
            {
                ForgetHandle(session);
                _flames.Remove(ownerId);
            }
        }

        private bool ShouldHardStopFlame()
        {
            return !_weaponRuntime.IsInitialized ||
                   !_weaponRuntime.GameplayEnabled ||
                   _weaponRuntime.CurrentWeapon != WeaponType.Flamethrower;
        }

        private void HardStop(
            EntityId ownerId,
            FlameSession session)
        {
            if (session == null)
            {
                _flames.Remove(ownerId);
                return;
            }

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
            session.DesiredActive = false;
            _flames.Remove(ownerId);
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

            // Guard against a weapon change that happened without another flame
            // callback between completion and this point.
            if (ShouldHardStopFlame())
            {
                _flames.Remove(ownerId);
                return;
            }

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
