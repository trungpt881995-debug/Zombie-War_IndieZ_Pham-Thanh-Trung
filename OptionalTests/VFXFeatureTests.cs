using System; using System.Collections.Generic; using NUnit.Framework; using ZombieWar.Features.VFX.Catalog; using ZombieWar.Features.VFX.Controller; using ZombieWar.Features.VFX.Model; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Features.VFX.Tests
{
    public sealed class VFXFeatureTests
    {
        private static VFXDefinition Def(VFXId id=VFXId.BulletImpact,VFXLifetimeMode life=VFXLifetimeMode.OneShot,float duration=1f,bool terminal=true)=>new VFXDefinition(id,life,duration,terminal,1,8,true,1f);
        private static VFXRequest Req(VFXId id=VFXId.BulletImpact,IVFXAnchor anchor=null){var p=VFXPoint.Zero;var pose=VFXPose.At(in p);return new VFXRequest(id,in pose,0f,anchor);}
        private static VFXRuntime Runtime(FakePools pools=null,params VFXDefinition[] defs){var r=new VFXRuntime(new VFXController(new VFXModel()));var d=defs==null||defs.Length==0?new[]{Def()}:defs;r.Initialize(new VFXCatalog(d),pools??new FakePools());r.SetMode(VFXGameplayMode.Playing);return r;}
        [Test] public void Catalog_Count(){var c=new VFXCatalog(new[]{Def(),Def(VFXId.BloodImpact)});Assert.AreEqual(2,c.Count);}
        [Test] public void Catalog_Duplicate_Throws(){Assert.Throws<ArgumentException>(()=>new VFXCatalog(new[]{Def(),Def()}));}
        [Test] public void Definition_None_Throws(){Assert.Throws<ArgumentOutOfRangeException>(()=>new VFXDefinition(VFXId.None,VFXLifetimeMode.OneShot,1,true,0,1,true,1));}
        [Test] public void Definition_OneShot_NonPositiveDuration_Throws(){Assert.Throws<ArgumentOutOfRangeException>(()=>new VFXDefinition(VFXId.BulletImpact,VFXLifetimeMode.OneShot,0,true,0,1,true,1));}
        [Test] public void Definition_Looping_ZeroDuration_OK(){Assert.DoesNotThrow(()=>new VFXDefinition(VFXId.FlamethrowerLoop,VFXLifetimeMode.Looping,0,false,0,1,true,1));}
        [Test] public void Runtime_Initialize(){var r=Runtime();Assert.IsTrue(r.IsInitialized);}
        [Test] public void Runtime_Shutdown(){var r=Runtime();((IVFXRuntimeConfigurator)r).Shutdown();Assert.IsFalse(r.IsInitialized);}
        [Test] public void Play_ReturnsHandle(){var r=Runtime();Assert.IsTrue(r.Play(Req()).IsValid);}
        [Test] public void Play_IncrementsActive(){var r=Runtime();r.Play(Req());Assert.AreEqual(1,r.ActiveCount);}
        [Test] public void OneShot_ReleasesAtDuration(){var r=Runtime();r.Play(Req());r.Tick(1f);Assert.AreEqual(0,r.ActiveCount);}
        [Test] public void OneShot_RemainsBeforeDuration(){var r=Runtime();r.Play(Req());r.Tick(.5f);Assert.AreEqual(1,r.ActiveCount);}
        [Test] public void Looping_DoesNotAutoRelease(){var r=Runtime(null,Def(VFXId.FlamethrowerLoop,VFXLifetimeMode.Looping,0,false));r.Play(Req(VFXId.FlamethrowerLoop));r.Tick(100f);Assert.AreEqual(1,r.ActiveCount);}
        [Test] public void Looping_Stop_Releases(){var r=Runtime(null,Def(VFXId.FlamethrowerLoop,VFXLifetimeMode.Looping,0,false));var h=r.Play(Req(VFXId.FlamethrowerLoop));Assert.IsTrue(r.Stop(h));Assert.AreEqual(0,r.ActiveCount);}
        [Test] public void Stop_Unknown_False(){var r=Runtime();Assert.IsFalse(r.Stop(new VFXHandle(99)));}
        [Test] public void CancelAll_Releases(){var r=Runtime();r.Play(Req());r.Play(Req());r.CancelAll();Assert.AreEqual(0,r.ActiveCount);}
        [Test] public void Inactive_Rejects(){var r=Runtime();r.SetMode(VFXGameplayMode.Inactive);Assert.IsFalse(r.Play(Req()).IsValid);}
        [Test] public void Suspended_RejectsNew(){var r=Runtime();r.SetMode(VFXGameplayMode.Suspended);Assert.IsFalse(r.Play(Req()).IsValid);}
        [Test] public void Suspended_FreezesLifetime(){var r=Runtime();r.Play(Req());r.SetMode(VFXGameplayMode.Suspended);r.Tick(10);Assert.AreEqual(1,r.ActiveCount);}
        [Test] public void Resume_ContinuesLifetime(){var r=Runtime();r.Play(Req());r.SetMode(VFXGameplayMode.Suspended);r.Tick(10);r.SetMode(VFXGameplayMode.Playing);r.Tick(1);Assert.AreEqual(0,r.ActiveCount);}
        [Test] public void TerminalDrain_AllowsSafeOneShot(){var r=Runtime();r.SetMode(VFXGameplayMode.TerminalDrain);Assert.IsTrue(r.Play(Req()).IsValid);}
        [Test] public void TerminalDrain_RejectsUnsafeOneShot(){var r=Runtime(null,Def(VFXId.BulletImpact,VFXLifetimeMode.OneShot,1,false));r.SetMode(VFXGameplayMode.TerminalDrain);Assert.IsFalse(r.Play(Req()).IsValid);}
        [Test] public void TerminalDrain_RejectsLoop(){var r=Runtime(null,Def(VFXId.FlamethrowerLoop,VFXLifetimeMode.Looping,0,true));r.SetMode(VFXGameplayMode.TerminalDrain);Assert.IsFalse(r.Play(Req(VFXId.FlamethrowerLoop)).IsValid);}
        [Test] public void TerminalDrain_StopsExistingLoop(){var r=Runtime(null,Def(VFXId.FlamethrowerLoop,VFXLifetimeMode.Looping,0,false));r.Play(Req(VFXId.FlamethrowerLoop));r.SetMode(VFXGameplayMode.TerminalDrain);Assert.AreEqual(0,r.ActiveCount);}
        [Test] public void Inactive_CancelsExisting(){var r=Runtime();r.Play(Req());r.SetMode(VFXGameplayMode.Inactive);Assert.AreEqual(0,r.ActiveCount);}
        [Test] public void Snapshot_PlayedCount(){var r=Runtime();r.Play(Req());Assert.AreEqual(1,r.Snapshot.PlayedCount);}
        [Test] public void Snapshot_RejectedCount(){var r=Runtime();r.SetMode(VFXGameplayMode.Inactive);r.Play(Req());Assert.AreEqual(1,r.Snapshot.RejectedCount);}
        [Test] public void MissingDefinition_Rejected(){var r=Runtime();Assert.IsFalse(r.Play(Req(VFXId.BloodImpact)).IsValid);}
        [Test] public void PoolFailure_Rejected(){var p=new FakePools{Fail=true};var r=Runtime(p);Assert.IsFalse(r.Play(Req()).IsValid);}
        [Test] public void Release_ExactlyOnce(){var p=new FakePools();var r=Runtime(p);var h=r.Play(Req());r.Stop(h);r.Stop(h);Assert.AreEqual(1,p.Last.ReleaseCount);}
        [Test] public void Anchor_InitialPoseUsed(){var p=new FakePools();var a=new FakeAnchor();var r=Runtime(p);r.Play(Req(VFXId.BulletImpact,a));Assert.AreEqual(3,p.Last.ViewFake.LastPose.Position.X);}
        [Test] public void Anchor_FollowsOnTick(){var p=new FakePools();var a=new FakeAnchor();var r=Runtime(p);r.Play(Req(VFXId.BulletImpact,a));a.X=7;r.Tick(.1f);Assert.AreEqual(7,p.Last.ViewFake.LastPose.Position.X);}
        [Test] public void Pause_CallsViewPaused(){var p=new FakePools();var r=Runtime(p);r.Play(Req());r.SetMode(VFXGameplayMode.Suspended);Assert.IsTrue(p.Last.ViewFake.Paused);}
        [Test] public void Resume_CallsViewUnpaused(){var p=new FakePools();var r=Runtime(p);r.Play(Req());r.SetMode(VFXGameplayMode.Suspended);r.SetMode(VFXGameplayMode.Playing);Assert.IsFalse(p.Last.ViewFake.Paused);}
        [Test] public void Stop_CallsViewStop(){var p=new FakePools();var r=Runtime(p);var h=r.Play(Req());r.Stop(h);Assert.AreEqual(1,p.Last.ViewFake.StopCount);}
        [Test] public void CancelAll_CallsStop(){var p=new FakePools();var r=Runtime(p);r.Play(Req());r.CancelAll();Assert.AreEqual(1,p.Last.ViewFake.StopCount);}
        [Test] public void Scale_DefaultPassed(){var p=new FakePools();var r=Runtime(p);r.Play(Req());Assert.AreEqual(1f,p.Last.ViewFake.Scale);}
        [Test] public void Scale_RequestOverrides(){var p=new FakePools();var r=Runtime(p);var point=VFXPoint.Zero;var pose=VFXPose.At(in point);var q=new VFXRequest(VFXId.BulletImpact,in pose,2f);r.Play(in q);Assert.AreEqual(2f,p.Last.ViewFake.Scale);}
        [TestCase(VFXGameplayMode.Inactive)][TestCase(VFXGameplayMode.Playing)][TestCase(VFXGameplayMode.Suspended)][TestCase(VFXGameplayMode.TerminalDrain)] public void Mode_Set_RoundTrips(VFXGameplayMode m){var r=Runtime();r.SetMode(m);Assert.AreEqual(m,r.Mode);}
        [TestCase(VFXId.PistolMuzzle)][TestCase(VFXId.AKMuzzle)][TestCase(VFXId.ShotgunMuzzle)][TestCase(VFXId.SniperMuzzle)][TestCase(VFXId.GrenadeMuzzle)][TestCase(VFXId.FlamethrowerLoop)][TestCase(VFXId.BulletImpact)][TestCase(VFXId.BloodImpact)][TestCase(VFXId.SoldierDamage)][TestCase(VFXId.ZombieHit)][TestCase(VFXId.ZombieDeath)][TestCase(VFXId.GrenadeExplosion)][TestCase(VFXId.BossSpawn)][TestCase(VFXId.BossHit)][TestCase(VFXId.BossDeath)] public void VFXId_IsNotNone(VFXId id){Assert.AreNotEqual(VFXId.None,id);}
        private sealed class FakePools:IVFXPoolRegistry{public bool Fail;public FakeLease Last;public bool TryAcquire(VFXId id,out IVFXLease lease){if(Fail){lease=null;return false;}Last=new FakeLease();lease=Last;return true;}public void ReleaseAll(){} }
        private sealed class FakeLease:IVFXLease{public readonly FakeView ViewFake=new FakeView();public IVFXView View=>ViewFake;public bool IsReleased{get;private set;}public int ReleaseCount;public void Release(){if(IsReleased)return;IsReleased=true;ReleaseCount++;}}
        private sealed class FakeView:IVFXView{public VFXPose LastPose;public float Scale;public bool Paused;public int StopCount;public void Activate(in VFXPose p,float s){LastPose=p;Scale=s;}public void SetPose(in VFXPose p){LastPose=p;}public void Play(){}public void SetPaused(bool p){Paused=p;}public void Stop(){StopCount++;}public void Deactivate(){}}
        private sealed class FakeAnchor:IVFXAnchor{public float X=3;public bool IsValid=>true;public VFXPose Pose{get{var p=new VFXPoint(X,0,0);return VFXPose.At(in p);}}}
    }
}
