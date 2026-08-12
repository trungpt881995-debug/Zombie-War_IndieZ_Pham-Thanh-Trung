using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using NUnit.Framework;
using ZombieWar.Features.Camera.Catalog;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Events;
using ZombieWar.Features.Camera.Ports;
using ZombieWar.Features.Camera.Services;

namespace ZombieWar.Features.Camera.Tests
{
    public sealed class CameraFeatureTests
    {
        private static CameraProfile Profile(float damping = 0.3f) =>
            new CameraProfile(
                CameraProjectionMode.Perspective,
                50f, 10f, 0.1f, 300f,
                55f, 45f, 0f,
                0f, 12f, -10f,
                damping, damping, damping);

        [TestCase(-20f, 0f, -10f)]
        [TestCase(20f, 0f, 10f)]
        [TestCase(0f, -20f, -5f)]
        [TestCase(0f, 20f, 5f)]
        [TestCase(100f, 100f, 10f)]
        [TestCase(-100f, -100f, -10f)]
        public void Bounds_Clamp_Works(float x, float z, float expectedX)
        {
            var bounds = new CameraBounds(-10f, 10f, -5f, 5f);
            var point = new CameraPoint(x, 2f, z);
            CameraPoint result = bounds.Clamp(in point);
            if (x < -10f) Assert.AreEqual(-10f, result.X);
            else if (x > 10f) Assert.AreEqual(10f, result.X);
            else Assert.AreEqual(expectedX, result.X);
            Assert.That(result.Z, Is.InRange(-5f, 5f));
        }

        [Test] public void Bounds_Inside_Point_Unchanged()
        {
            var bounds = new CameraBounds(-10, 10, -5, 5); var point = new CameraPoint(2, 3, 4);
            Assert.AreEqual(point, bounds.Clamp(in point));
        }

        [Test] public void Bounds_Invalid_Returns_Point()
        {
            var bounds = new CameraBounds(1, 1, 0, 5); var point = new CameraPoint(9, 3, 4);
            Assert.AreEqual(point, bounds.Clamp(in point));
        }

        [Test] public void Profile_Rejects_Invalid_Fov() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraProfile(CameraProjectionMode.Perspective, 180, 10, .1f, 100, 0,0,0, 0,0,0, 0,0,0));

        [Test] public void Profile_Rejects_Invalid_Clip() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraProfile(CameraProjectionMode.Perspective, 50, 10, 10, 1, 0,0,0, 0,0,0, 0,0,0));

        [Test] public void Profile_Rejects_Negative_Damping() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Profile(-1f));

        [Test] public void ShakeDefinition_Rejects_None() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraShakeDefinition(CameraShakeId.None, 1, 1, 1));

        [Test] public void ShakeCatalog_Rejects_Duplicates()
        {
            var d = new CameraShakeDefinition(CameraShakeId.Explosion, 1, 1, 1);
            Assert.Throws<ArgumentException>(() => new CameraShakeCatalog(new[] { d, d }));
        }

        [Test] public void Runtime_Initial_State_Uninitialized()
        {
            var runtime = new CameraRuntime(new EventBus()); Assert.AreEqual(CameraState.Uninitialized, runtime.State);
        }

        [Test] public void Initialize_Moves_To_Ready()
        {
            TestContext c = Create(); Assert.AreEqual(CameraState.Ready, c.Runtime.State); Assert.IsTrue(c.Rig.ProfileApplied);
        }

        [Test] public void Enable_Moves_To_Active()
        {
            TestContext c = Create(); c.Runtime.SetGameplayEnabled(true); Assert.AreEqual(CameraState.Active, c.Runtime.State); Assert.IsTrue(c.Rig.Enabled);
        }

        [Test] public void Disable_Moves_To_Suspended_And_Stops_Shake()
        {
            TestContext c = Create(); c.Runtime.SetGameplayEnabled(true); c.Runtime.SetGameplayEnabled(false);
            Assert.AreEqual(CameraState.Suspended, c.Runtime.State); Assert.AreEqual(1, c.Shake.StopCount);
        }

        [Test] public void Tick_Acquires_Target()
        {
            TestContext c = Create(); c.Target.Set(2, 0, 3); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f);
            Assert.IsTrue(c.Runtime.HasTarget); Assert.AreEqual(new CameraPoint(2,0,3), c.Runtime.RawTarget);
        }

        [Test] public void Tick_Without_Target_Is_Safe()
        {
            TestContext c = Create(); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f); Assert.IsFalse(c.Runtime.HasTarget); Assert.AreEqual(0, c.Rig.SetCount);
        }

        [Test] public void Tick_Clamps_Target_To_Bounds()
        {
            TestContext c = Create(); c.Target.Set(50,0,50); c.Bounds.Set(new CameraBounds(-10,10,-5,5)); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f);
            Assert.AreEqual(new CameraPoint(10,0,5), c.Runtime.ConstrainedTarget);
        }

        [Test] public void Tick_Without_Bounds_Uses_Raw_Target()
        {
            TestContext c = Create(); c.Target.Set(50,0,50); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f);
            Assert.AreEqual(c.Runtime.RawTarget, c.Runtime.ConstrainedTarget);
        }

        [Test] public void Rig_Receives_Constrained_Target()
        {
            TestContext c = Create(); c.Target.Set(50,0,50); c.Bounds.Set(new CameraBounds(-10,10,-5,5)); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f);
            Assert.AreEqual(new CameraPoint(10,0,5), c.Rig.LastTarget);
        }

        [Test] public void Disabled_Runtime_Does_Not_Update_Rig()
        {
            TestContext c = Create(); c.Target.Set(1,0,1); c.Runtime.Tick(.016f); Assert.AreEqual(0, c.Rig.SetCount);
        }

        [Test] public void Snap_Uses_Constrained_Target()
        {
            TestContext c = Create(); c.Target.Set(50,0,50); c.Bounds.Set(new CameraBounds(-10,10,-5,5)); Assert.IsTrue(c.Runtime.SnapToTarget());
            Assert.AreEqual(new CameraPoint(10,0,5), c.Rig.LastSnap);
        }

        [Test] public void Snap_Without_Target_Returns_False()
        {
            TestContext c = Create(); Assert.IsFalse(c.Runtime.SnapToTarget()); Assert.AreEqual(0, c.Rig.SnapCount);
        }

        [Test] public void ApplyProfile_Propagates_To_Rig()
        {
            TestContext c = Create(); var profile = new CameraProfile(CameraProjectionMode.Orthographic, 60, 14, .2f, 500, 60,20,0, 1,2,3, .4f,.5f,.6f);
            c.Runtime.ApplyProfile(in profile); Assert.AreEqual(profile, c.Rig.Profile); Assert.AreEqual(profile, c.Runtime.Profile);
        }

        [Test] public void Same_Profile_Does_Not_Reapply()
        {
            TestContext c = Create(); int before = c.Rig.ApplyCount; CameraProfile profile = c.Runtime.Profile; c.Runtime.ApplyProfile(in profile); Assert.AreEqual(before, c.Rig.ApplyCount);
        }

        [Test] public void Shake_Active_Known_Id_Plays()
        {
            TestContext c = Create(withShake: true); c.Runtime.SetGameplayEnabled(true); Assert.IsTrue(c.Runtime.TryRequestShake(CameraShakeId.Explosion)); Assert.AreEqual(1, c.Shake.PlayCount);
        }

        [Test] public void Shake_Unknown_Id_Fails_Safely()
        {
            TestContext c = Create(withShake: true); c.Runtime.SetGameplayEnabled(true); Assert.IsFalse(c.Runtime.TryRequestShake(CameraShakeId.BossImpact));
        }

        [Test] public void Shake_Disabled_Runtime_Is_Ignored()
        {
            TestContext c = Create(withShake: true); Assert.IsFalse(c.Runtime.TryRequestShake(CameraShakeId.Explosion)); Assert.AreEqual(0, c.Shake.PlayCount);
        }

        [Test] public void TargetBound_Event_Fires_Once_Until_Target_Lost()
        {
            TestContext c = Create(); int count = 0; c.Bus.Subscribe<CameraTargetBoundEvent>(_ => count++);
            c.Target.Set(1,0,1); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f); c.Runtime.Tick(.016f); Assert.AreEqual(1, count);
        }

        [Test] public void BoundsChanged_Event_Does_Not_Spam_When_Unchanged()
        {
            TestContext c = Create(); int count = 0; c.Bus.Subscribe<CameraBoundsChangedEvent>(_ => count++);
            c.Bounds.Set(new CameraBounds(-1,1,-1,1)); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f); c.Runtime.Tick(.016f); Assert.AreEqual(1, count);
        }

        [Test] public void BoundsChanged_Event_Fires_When_Replaced()
        {
            TestContext c = Create(); int count = 0; c.Bus.Subscribe<CameraBoundsChangedEvent>(_ => count++);
            c.Bounds.Set(new CameraBounds(-1,1,-1,1)); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f);
            c.Bounds.Set(new CameraBounds(-2,2,-2,2)); c.Runtime.Tick(.016f); Assert.AreEqual(2, count);
        }

        [Test] public void Bounds_Removal_Clears_HasBounds()
        {
            TestContext c = Create(); c.Bounds.Set(new CameraBounds(-1,1,-1,1)); c.Runtime.SetGameplayEnabled(true); c.Runtime.Tick(.016f); Assert.IsTrue(c.Runtime.HasBounds);
            c.Bounds.Clear(); c.Runtime.Tick(.016f); Assert.IsFalse(c.Runtime.HasBounds);
        }

        [Test] public void Shutdown_Resets_Runtime()
        {
            TestContext c = Create(); c.Configurator.Shutdown(); Assert.AreEqual(CameraState.Uninitialized, c.Runtime.State); Assert.IsFalse(c.Runtime.IsInitialized);
        }

        [Test] public void Initialize_Rejects_NotReady_Rig()
        {
            var runtime = new CameraRuntime(new EventBus()); var target = new FakeTarget(); var bounds = new FakeBounds(); var rig = new FakeRig { Ready = false };
            CameraProfile p = Profile(); Assert.Throws<InvalidOperationException>(() => ((ICameraRuntimeConfigurator)runtime).Initialize(in p, new CameraShakeCatalog(Array.Empty<CameraShakeDefinition>()), target, bounds, rig, new FakeShake()));
        }

        [Test] public void CameraPoint_Rejects_Nan() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new CameraPoint(float.NaN, 0, 0));

        [Test] public void Bounds_Contains_Works()
        {
            var b = new CameraBounds(-1,1,-1,1); var inside = new CameraPoint(0,0,0); var outside = new CameraPoint(2,0,0);
            Assert.IsTrue(b.Contains(in inside)); Assert.IsFalse(b.Contains(in outside));
        }

        [Test] public void Runtime_Sanitizes_Negative_Delta()
        {
            TestContext c = Create(); c.Target.Set(1,0,1); c.Runtime.SetGameplayEnabled(true); Assert.DoesNotThrow(() => c.Runtime.Tick(-1f));
        }

        private static TestContext Create(bool withShake = false)
        {
            var bus = new EventBus(); var runtime = new CameraRuntime(bus); var target = new FakeTarget(); var bounds = new FakeBounds(); var rig = new FakeRig(); var shake = new FakeShake();
            CameraProfile p = Profile();
            ICameraShakeCatalog catalog = withShake
                ? new CameraShakeCatalog(new[] { new CameraShakeDefinition(CameraShakeId.Explosion, 1, 2, .25f) })
                : new CameraShakeCatalog(Array.Empty<CameraShakeDefinition>());
            ICameraRuntimeConfigurator configurator = runtime;
            configurator.Initialize(in p, catalog, target, bounds, rig, shake);
            return new TestContext(bus, runtime, configurator, target, bounds, rig, shake);
        }

        private sealed class TestContext
        {
            public EventBus Bus { get; }
            public CameraRuntime Runtime { get; }
            public ICameraRuntimeConfigurator Configurator { get; }
            public FakeTarget Target { get; }
            public FakeBounds Bounds { get; }
            public FakeRig Rig { get; }
            public FakeShake Shake { get; }
            public TestContext(EventBus bus, CameraRuntime runtime, ICameraRuntimeConfigurator configurator, FakeTarget target, FakeBounds bounds, FakeRig rig, FakeShake shake)
            { Bus=bus; Runtime=runtime; Configurator=configurator; Target=target; Bounds=bounds; Rig=rig; Shake=shake; }
        }

        private sealed class FakeTarget : ICameraTargetProvider
        {
            private bool _has; private CameraPoint _point;
            public void Set(float x,float y,float z){ _point=new CameraPoint(x,y,z); _has=true; }
            public void Clear()=>_has=false;
            public bool TryGetTarget(out CameraPoint position){ position=_point; return _has; }
        }

        private sealed class FakeBounds : ICameraBoundsProvider
        {
            private bool _has; private CameraBounds _bounds;
            public void Set(CameraBounds value){ _bounds=value; _has=true; }
            public void Clear()=>_has=false;
            public bool TryGetBounds(out CameraBounds bounds){ bounds=_bounds; return _has; }
        }

        private sealed class FakeRig : ICameraRig
        {
            public bool Ready=true; public bool IsReady=>Ready; public bool Enabled; public bool ProfileApplied; public int ApplyCount; public int SetCount; public int SnapCount; public CameraProfile Profile; public CameraPoint LastTarget; public CameraPoint LastSnap;
            public void ApplyProfile(in CameraProfile profile){ Profile=profile; ProfileApplied=true; ApplyCount++; }
            public void SetTarget(in CameraPoint target){ LastTarget=target; SetCount++; }
            public void SnapToTarget(in CameraPoint target){ LastSnap=target; SnapCount++; }
            public void SetEnabled(bool enabled){ Enabled=enabled; }
        }

        private sealed class FakeShake : ICameraShakeDriver
        {
            public int PlayCount; public int StopCount; public CameraShakeRequest Last;
            public bool TryPlay(in CameraShakeRequest request){ Last=request; PlayCount++; return true; }
            public void StopAll(){ StopCount++; }
        }
    }
}
