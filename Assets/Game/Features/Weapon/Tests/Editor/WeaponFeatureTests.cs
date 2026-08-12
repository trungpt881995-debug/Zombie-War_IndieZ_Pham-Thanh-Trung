using System;
using System.Collections.Generic;
using NUnit.Framework;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Catalog;
using ZombieWar.Features.Weapon.Controller;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Events;
using ZombieWar.Features.Weapon.Factories;
using ZombieWar.Features.Weapon.Model;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Features.Weapon.Services;
using ZombieWar.Features.Weapon.Strategies;
using ZombieWar.Features.Weapon.View;

namespace ZombieWar.Features.Weapon.Tests
{
    public sealed class WeaponFeatureTests
    {
        [Test] public void Catalog_ContainsExactlySixWeapons()
        { Assert.AreEqual(6, CreateCatalog().Count); }

        [Test] public void Catalog_DuplicateWeapon_Throws()
        {
            var defs = CreateDefinitions();
            defs[5] = defs[0];
            Assert.Throws<ArgumentException>(() => new WeaponCatalog(defs));
        }

        [Test] public void Runtime_InitialWeapon_IsConfigurable()
        {
            WeaponRuntime runtime = CreateRuntime();
            runtime.Initialize(CreateCatalog(), WeaponType.SniperRifle);
            Assert.AreEqual(WeaponType.SniperRifle, runtime.CurrentWeapon);
        }

        [Test] public void Select_NewWeapon_Succeeds()
        {
            WeaponRuntime runtime = InitializedRuntime();
            WeaponSelectionResult result = runtime.TrySelect(WeaponType.AK);
            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(WeaponType.AK, runtime.CurrentWeapon);
        }

        [Test] public void Reselect_CurrentWeapon_IsNoOp()
        {
            WeaponRuntime runtime = InitializedRuntime();
            WeaponSelectionResult result = runtime.TrySelect(WeaponType.Pistol);
            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(WeaponSelectionRejectReason.AlreadySelected, result.RejectReason);
        }

        [Test] public void PreviousWeapon_StartsSelectionCooldown()
        {
            WeaponRuntime runtime = InitializedRuntime();
            runtime.TrySelect(WeaponType.AK);
            Assert.AreEqual(2f, runtime.Cooldowns.Get(WeaponType.Pistol), 0.0001f);
        }

        [Test] public void WeaponOnCooldown_CannotBeSelected()
        {
            WeaponRuntime runtime = InitializedRuntime();
            runtime.TrySelect(WeaponType.AK);
            runtime.TrySelect(WeaponType.Shotgun);
            WeaponSelectionResult result = runtime.TrySelect(WeaponType.AK);
            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(WeaponSelectionRejectReason.OnCooldown, result.RejectReason);
        }

        [Test] public void Cooldown_TicksToZero()
        {
            WeaponRuntime runtime = InitializedRuntime();
            runtime.TrySelect(WeaponType.AK);
            runtime.Tick(2.1f);
            Assert.AreEqual(0f, runtime.Cooldowns.Get(WeaponType.Pistol), 0.0001f);
        }

        [Test] public void Pause_FreezesSelectionCooldown()
        {
            WeaponRuntime runtime = InitializedRuntime();
            runtime.TrySelect(WeaponType.AK);
            runtime.SetGameplayEnabled(false);
            runtime.Tick(10f);
            Assert.AreEqual(2f, runtime.Cooldowns.Get(WeaponType.Pistol), 0.0001f);
        }

        [Test] public void DisabledGameplay_RejectsSelection()
        {
            WeaponRuntime runtime = InitializedRuntime();
            runtime.SetGameplayEnabled(false);
            Assert.AreEqual(WeaponSelectionRejectReason.GameplayDisabled,
                runtime.TrySelect(WeaponType.AK).RejectReason);
        }

        [Test] public void ResetForGameLevel_ClearsCooldownsAndInitialWeapon()
        {
            WeaponRuntime runtime = InitializedRuntime();
            runtime.TrySelect(WeaponType.AK);
            runtime.ResetForGameLevel();
            Assert.AreEqual(WeaponType.Pistol, runtime.CurrentWeapon);
            Assert.AreEqual(0f, runtime.Cooldowns.Get(WeaponType.Pistol));
        }

        [Test] public void TargetRange_FollowsCurrentWeaponImmediately()
        {
            WeaponRuntime runtime = InitializedRuntime();
            float pistol = runtime.CurrentTargetRange;
            runtime.TrySelect(WeaponType.SniperRifle);
            Assert.Greater(runtime.CurrentTargetRange, pistol);
        }

        [Test] public void SingleProjectile_FiresExactlyOneRequest()
        {
            var p = new FakeProjectilePort();
            var s = new SingleProjectileFireStrategy(p, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.Pistol);
            WeaponFireContext c = Context(1, 100);
            Assert.IsTrue(s.Fire(in d, in c));
            Assert.AreEqual(1, p.Requests.Count);
        }

        [Test] public void Shotgun_FiresExactlySevenRequests()
        {
            var p = new FakeProjectilePort();
            var s = new ShotgunFireStrategy(p, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.Shotgun);
            WeaponFireContext c = Context(1, 100);
            Assert.IsTrue(s.Fire(in d, in c));
            Assert.AreEqual(7, p.Requests.Count);
        }

        [Test] public void Shotgun_CenterPellet_IsStraight()
        {
            var p = new FakeProjectilePort();
            var s = new ShotgunFireStrategy(p, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.Shotgun);
            WeaponFireContext c = Context(1, 100);
            s.Fire(in d, in c);
            WeaponDirection center = p.Requests[3].Direction;
            Assert.AreEqual(0f, center.X, 0.0001f);
            Assert.Greater(center.Z, 0.99f);
        }

        [Test] public void Shotgun_Directions_AreSymmetric()
        {
            var p = new FakeProjectilePort();
            var s = new ShotgunFireStrategy(p, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.Shotgun);
            WeaponFireContext c = Context(1, 100);
            s.Fire(in d, in c);
            Assert.AreEqual(-p.Requests[0].Direction.X, p.Requests[6].Direction.X, 0.0001f);
        }

        [Test] public void Sniper_UsesSniperProjectileProfile()
        {
            var p = new FakeProjectilePort();
            var s = new SingleProjectileFireStrategy(p, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.SniperRifle);
            WeaponFireContext c = Context(1, 100);
            s.Fire(in d, in c);
            Assert.AreEqual(WeaponProjectileProfileId.SniperBullet, p.Requests[0].Profile);
        }

        [Test] public void Grenade_PassesTargetPointAndExplosionRadius()
        {
            var p = new FakeProjectilePort();
            var s = new GrenadeFireStrategy(p, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.GrenadeLauncher);
            WeaponFireContext c = Context(1, 100);
            s.Fire(in d, in c);
            Assert.IsTrue(p.Requests[0].HasTargetPoint);
            Assert.AreEqual(d.ExplosionRadius, p.Requests[0].ExplosionRadius);
        }

        [Test] public void Flamethrower_FiresNoProjectile()
        {
            var p = new FakeProjectilePort();
            var flame = new FakeFlamePort();
            var s = new FlamethrowerFireStrategy(flame, NullWeaponFeedbackPort.Instance);
            WeaponDefinition d = Definition(WeaponType.Flamethrower);
            WeaponFireContext c = Context(1, 100);
            s.OnTargetAcquired(in d, in c);
            s.Fire(in d, in c);
            Assert.AreEqual(0, p.Requests.Count);
            Assert.AreEqual(1, flame.BeginCount);
            Assert.AreEqual(1, flame.TickCount);
        }

        [Test] public void Flamethrower_EndsOnTargetClear()
        {
            var flame = new FakeFlamePort();
            var s = new FlamethrowerFireStrategy(flame, NullWeaponFeedbackPort.Instance);
            s.OnTargetCleared(new EntityId(5));
            Assert.AreEqual(1, flame.EndCount);
        }

        [Test] public void FireSessions_AreIndependentPerSoldier()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget target = Target(100);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            fixture.Service.Update(new EntityId(2), in target, 0f);
            Assert.AreEqual(2, fixture.Projectiles.Requests.Count);
        }

        [Test] public void FireCadence_BlocksSecondImmediateShot()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget target = Target(100);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            Assert.AreEqual(1, fixture.Projectiles.Requests.Count);
        }

        [Test] public void FireCadence_AllowsShotAfterInterval()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget target = Target(100);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            fixture.Service.Update(new EntityId(1), in target, 0.5f);
            Assert.AreEqual(2, fixture.Projectiles.Requests.Count);
        }

        [Test] public void TargetChange_AllowsImmediateShot()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget a = Target(100);
            WeaponTarget b = Target(101);
            fixture.Service.Update(new EntityId(1), in a, 0f);
            fixture.Service.Update(new EntityId(1), in b, 0f);
            Assert.AreEqual(2, fixture.Projectiles.Requests.Count);
        }

        [Test] public void WeaponSwitch_AllowsImmediateNewWeaponShot()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget target = Target(100);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            fixture.Runtime.TrySelect(WeaponType.AK);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            Assert.AreEqual(2, fixture.Projectiles.Requests.Count);
            Assert.AreEqual(WeaponProjectileProfileId.AKBullet, fixture.Projectiles.Requests[1].Profile);
        }

        [Test] public void OwnerId_IsPropagatedIntoProjectileRequest()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget target = Target(100);
            fixture.Service.Update(new EntityId(77), in target, 0f);
            Assert.AreEqual(77, fixture.Projectiles.Requests[0].OwnerId.Value);
        }

        [Test] public void MuzzlePosition_IsUsedAsProjectileOrigin()
        {
            var fixture = CreateAttackFixture();
            WeaponTarget target = Target(100);
            fixture.Service.Update(new EntityId(1), in target, 0f);
            Assert.AreEqual(2f, fixture.Projectiles.Requests[0].Origin.X, 0.0001f);
        }

        [Test] public void MissingMuzzle_DoesNotFire()
        {
            WeaponRuntime runtime = InitializedRuntime();
            var projectiles = new FakeProjectilePort();
            var provider = new WeaponFireStrategyProvider(projectiles, NullWeaponFlamePort.Instance, NullWeaponFeedbackPort.Instance);
            var service = new WeaponAttackService(runtime, new EmptyMuzzleProvider(), provider, new WeaponFireSessionFactory());
            WeaponTarget target = Target(100);
            service.Update(new EntityId(1), in target, 1f);
            Assert.AreEqual(0, projectiles.Requests.Count);
        }

        [Test] public void ClearTarget_StopsFlamethrower()
        {
            WeaponRuntime runtime = InitializedRuntime(WeaponType.Flamethrower);
            var flame = new FakeFlamePort();
            var provider = new WeaponFireStrategyProvider(new FakeProjectilePort(), flame, NullWeaponFeedbackPort.Instance);
            var service = new WeaponAttackService(runtime, new FakeMuzzleProvider(), provider, new WeaponFireSessionFactory());
            WeaponTarget target = Target(100);
            service.Update(new EntityId(1), in target, 0f);
            service.ClearTarget(new EntityId(1));
            Assert.AreEqual(1, flame.EndCount);
        }

        [TestCase(WeaponType.Pistol, 2.5f)]
        [TestCase(WeaponType.AK, 10f)]
        [TestCase(WeaponType.Shotgun, 1f)]
        [TestCase(WeaponType.SniperRifle, 0.8f)]
        [TestCase(WeaponType.GrenadeLauncher, 0.6f)]
        public void ProjectileWeapon_FireIntervalMatchesConfiguredRate(WeaponType type, float expectedRate)
        {
            WeaponDefinition d = Definition(type);
            Assert.AreEqual(1f / expectedRate, d.FireInterval, 0.0001f);
        }

        [Test] public void Flamethrower_UsesTickIntervalInsteadOfFireRate()
        {
            WeaponDefinition d = Definition(WeaponType.Flamethrower);
            Assert.AreEqual(0.1f, d.FireInterval, 0.0001f);
        }

        [Test] public void SelectionEvents_FireOnlyOnAcceptedTransition()
        {
            var bus = new EventBus();
            int selected = 0;
            using (bus.Subscribe<WeaponSelectedEvent>(_ => selected++))
            {
                var runtime = new WeaponRuntime(bus, NullWeaponView.Instance);
                runtime.Initialize(CreateCatalog(), WeaponType.Pistol);
                runtime.TrySelect(WeaponType.Pistol);
                runtime.TrySelect(WeaponType.AK);
                Assert.AreEqual(1, selected);
            }
        }

        [Test] public void CooldownReadyEvent_FiresOnce()
        {
            var bus = new EventBus();
            int ready = 0;
            using (bus.Subscribe<WeaponCooldownReadyEvent>(_ => ready++))
            {
                var runtime = new WeaponRuntime(bus, NullWeaponView.Instance);
                runtime.Initialize(CreateCatalog(), WeaponType.Pistol);
                runtime.TrySelect(WeaponType.AK);
                runtime.Tick(3f);
                runtime.Tick(3f);
                Assert.AreEqual(1, ready);
            }
        }

        private static WeaponRuntime CreateRuntime() =>
            new WeaponRuntime(new EventBus(), NullWeaponView.Instance);

        private static WeaponRuntime InitializedRuntime(WeaponType initial = WeaponType.Pistol)
        {
            WeaponRuntime r = CreateRuntime();
            r.Initialize(CreateCatalog(), initial);
            return r;
        }

        private static WeaponCatalog CreateCatalog() => new WeaponCatalog(CreateDefinitions());

        private static WeaponDefinition[] CreateDefinitions() => new[]
        {
            Definition(WeaponType.Pistol), Definition(WeaponType.AK), Definition(WeaponType.Shotgun),
            Definition(WeaponType.SniperRifle), Definition(WeaponType.GrenadeLauncher), Definition(WeaponType.Flamethrower)
        };

        private static WeaponDefinition Definition(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Pistol: return new WeaponDefinition(type,10,2.5f,20,25,10,2,0,3,0,0,0);
                case WeaponType.AK: return new WeaponDefinition(type,8,10,25,30,12,4,0,3,0,0,0);
                case WeaponType.Shotgun: return new WeaponDefinition(type,5,1,18,15,8,6,30,2,0,0,0);
                case WeaponType.SniperRifle: return new WeaponDefinition(type,30,0.8f,60,50,30,7,0,2,0,0,0);
                case WeaponType.GrenadeLauncher: return new WeaponDefinition(type,40,0.6f,10,40,20,9,0,6,3,0,0);
                case WeaponType.Flamethrower: return new WeaponDefinition(type,3,0,0,0,8,10,0,0,0,0.1f,1.5f);
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static WeaponFireContext Context(long owner, long target)
        {
            var p = new WeaponPoint(0,0,0);
            var f = new WeaponDirection(0,0,1);
            var m = new WeaponMuzzle(in p, in f);
            var tp = new WeaponPoint(0,0,10);
            var t = new WeaponTarget(new EntityId(target), in tp);
            return new WeaponFireContext(new EntityId(owner), in m, in t);
        }

        private static WeaponTarget Target(long id)
        {
            var p = new WeaponPoint(0,0,10);
            return new WeaponTarget(new EntityId(id), in p);
        }

        private static AttackFixture CreateAttackFixture()
        {
            WeaponRuntime runtime = InitializedRuntime();
            var projectiles = new FakeProjectilePort();
            var provider = new WeaponFireStrategyProvider(projectiles, NullWeaponFlamePort.Instance, NullWeaponFeedbackPort.Instance);
            var service = new WeaponAttackService(runtime, new FakeMuzzleProvider(), provider, new WeaponFireSessionFactory());
            return new AttackFixture(runtime, projectiles, service);
        }

        private sealed class AttackFixture
        {
            public readonly WeaponRuntime Runtime;
            public readonly FakeProjectilePort Projectiles;
            public readonly WeaponAttackService Service;
            public AttackFixture(WeaponRuntime runtime, FakeProjectilePort projectiles, WeaponAttackService service)
            { Runtime = runtime; Projectiles = projectiles; Service = service; }
        }

        private sealed class FakeProjectilePort : IWeaponProjectilePort
        {
            public readonly List<WeaponProjectileRequest> Requests = new List<WeaponProjectileRequest>();
            public bool TryLaunch(in WeaponProjectileRequest request) { Requests.Add(request); return true; }
        }

        private sealed class FakeFlamePort : IWeaponFlamePort
        {
            public int BeginCount, TickCount, EndCount;
            public void Begin(in WeaponFlameRequest request) => BeginCount++;
            public void ApplyTick(in WeaponFlameRequest request) => TickCount++;
            public void End(EntityId ownerId) => EndCount++;
        }

        private sealed class FakeMuzzleProvider : IWeaponMuzzleProvider
        {
            public bool TryGetMuzzle(EntityId ownerId, out WeaponMuzzle muzzle)
            {
                var p = new WeaponPoint(2,0,0);
                var d = new WeaponDirection(0,0,1);
                muzzle = new WeaponMuzzle(in p, in d);
                return true;
            }
        }

        private sealed class EmptyMuzzleProvider : IWeaponMuzzleProvider
        { public bool TryGetMuzzle(EntityId ownerId, out WeaponMuzzle muzzle) { muzzle = default; return false; } }
    }
}
