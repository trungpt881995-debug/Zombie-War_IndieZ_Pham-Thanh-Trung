using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using NUnit.Framework;
using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Events;
using ZombieWar.Features.Zombie.Factories;
using ZombieWar.Features.Zombie.Ports;
using ZombieWar.Features.Zombie.Registry;

namespace ZombieWar.Features.Zombie.Tests
{
    public sealed class ZombieFeatureTests
    {
        private static ZombieDefinition Def(float spawn=0f, float hit=0.1f, float death=0.1f, float dissolve=0.1f) =>
            new ZombieDefinition(100f, 2f, 360f, 10f, 1.5f, 0.2f, 1f, 0f, spawn, hit, 0.2f, 1f, death, dissolve);

        [Test] public void Activate_AssignsFreshEntityId() { var a=Make(); var d=Def(); var r=Req(0,0,0); var id=a.C.Activate(in d,in r); Assert.AreEqual(1,id.Value); }
        [Test] public void Activate_RegistersTarget() { var a=Make(); var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); Assert.AreEqual(1,a.Reg.RegisterCount); }
        [Test] public void Activate_PublishesEvent() { var a=Make(); var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); Assert.AreEqual(1,a.Bus.Activated); }
        [Test] public void Spawn_TransitionsToChase() { var a=Make(); var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); a.C.Tick(0.01f); Assert.AreEqual(ZombieStateId.Chase,a.C.State); }
        [Test] public void SpawnDuration_IsRespected() { var a=Make(); var d=Def(spawn:1f); var r=Req(0,0,0); a.C.Activate(in d,in r); a.C.Tick(.5f); Assert.AreEqual(ZombieStateId.Spawn,a.C.State); }
        [Test] public void Chase_NoTarget_DoesNotMove() { var a=Make(); ActivateChase(a); a.C.Tick(.1f); Assert.AreEqual(0,a.Motor.MoveCount); }
        [Test] public void Chase_Target_Moves() { var a=Make(); ActivateChase(a); a.Target.Set(99,10,0,0); a.C.Tick(.1f); Assert.Greater(a.Motor.MoveCount,0); }
        [Test] public void Chase_InsideRange_EntersAttack() { var a=Make(); ActivateChase(a); a.Target.Set(99,1,0,0); a.C.Tick(.1f); Assert.AreEqual(ZombieStateId.Attack,a.C.State); }
        [Test] public void Attack_StopsMotor() { var a=Make(); EnterAttack(a); Assert.Greater(a.Motor.StopCount,0); }
        [Test] public void Attack_Impact_DamagesOnce() { var a=Make(); EnterAttack(a); a.C.Tick(.01f); a.C.NotifyAttackImpact(); a.C.NotifyAttackImpact(); Assert.AreEqual(1,a.Attack.Count); }
        [Test] public void Attack_TargetLeaves_ReturnsChase() { var a=Make(); EnterAttack(a); a.Target.Set(99,20,0,0); a.C.Tick(.01f); Assert.AreEqual(ZombieStateId.Chase,a.C.State); }
        [Test] public void Attack_TargetMissing_ReturnsChase() { var a=Make(); EnterAttack(a); a.Target.Clear(); a.C.Tick(.01f); Assert.AreEqual(ZombieStateId.Chase,a.C.State); }
        [Test] public void NonLethalDamage_EntersHit() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(5),10)); Assert.AreEqual(ZombieStateId.Hit,a.C.State); }
        [Test] public void Hit_FinishesToChase() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(5),10)); a.C.NotifyHitAnimationFinished(); Assert.AreEqual(ZombieStateId.Chase,a.C.State); }
        [Test] public void LethalDamage_EntersDeath() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(5),100)); Assert.AreEqual(ZombieStateId.Death,a.C.State); }
        [Test] public void Death_IsImmediatelyNotTargetable() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(5),100)); Assert.False(a.C.IsTargetable); }
        [Test] public void Death_UnregistersTargetImmediately() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(5),100)); Assert.AreEqual(1,a.Reg.UnregisterCount); }
        [Test] public void Death_PublishesKilledOnce() { var a=Make(); ActivateChase(a); var damage=new DamageInfo(new EntityId(5),100); a.C.ReceiveDamage(damage); a.C.ReceiveDamage(damage); Assert.AreEqual(1,a.Bus.Killed); }
        [Test] public void Death_PreservesKillerId() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(77),100)); Assert.AreEqual(77,a.Bus.LastKiller.Value); }
        [Test] public void Death_Dissolve_ReturnsPool() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(5),100)); a.C.NotifyDeathAnimationFinished(); a.C.Tick(.2f); Assert.AreEqual(1,a.Pool.ReturnCount); }
        [Test] public void Cancel_DoesNotPublishKilled() { var a=Make(); ActivateChase(a); a.C.Cancel(); Assert.AreEqual(0,a.Bus.Killed); Assert.AreEqual(1,a.Pool.ReturnCount); }
        [Test] public void Cancel_IsIdempotent() { var a=Make(); ActivateChase(a); a.C.Cancel(); a.C.Cancel(); Assert.AreEqual(1,a.Pool.ReturnCount); }
        [Test] public void Pause_StopsTickMovement() { var a=Make(); ActivateChase(a); a.Target.Set(99,10,0,0); a.C.SetGameplayEnabled(false); a.C.Tick(1f); Assert.AreEqual(0,a.Motor.MoveCount); }
        [Test] public void Resume_AllowsMovementAgain() { var a=Make(); ActivateChase(a); a.Target.Set(99,10,0,0); a.C.SetGameplayEnabled(false); a.C.SetGameplayEnabled(true); a.C.Tick(.1f); Assert.Greater(a.Motor.MoveCount,0); }
        [Test] public void ActiveRegistry_AddRemove() { var a=Make(); var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); var reg=new ActiveZombieRegistry(); Assert.True(reg.Add(a.C)); Assert.True(reg.Remove(a.C.EntityId)); Assert.AreEqual(0,reg.Count); }
        [Test] public void ActiveRegistry_RejectsDuplicate() { var a=Make(); var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); var reg=new ActiveZombieRegistry(); Assert.True(reg.Add(a.C)); Assert.False(reg.Add(a.C)); }
        [Test] public void PoolReuse_AfterDeactivate_GetsNewId() { var a=Make(); var d=Def(); var r=Req(0,0,0); var first=a.C.Activate(in d,in r); a.C.Cancel(); a.C.DeactivateForPool(); var second=a.C.Activate(in d,in r); Assert.AreNotEqual(first,second); }
        [Test] public void Health_IsResetOnActivate() { var a=Make(); var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); a.C.ReceiveDamage(new DamageInfo(new EntityId(1),20)); Assert.AreEqual(80,a.C.CurrentHealth); }
        [Test] public void DamageZero_IsIgnored() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(1),0)); Assert.AreEqual(100,a.C.CurrentHealth); }
        [Test] public void Death_DisablesCollision() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(1),100)); Assert.False(a.View.Collision); }
        [Test] public void Death_StopsMovement() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(1),100)); Assert.Greater(a.Motor.StopCount,0); }
        [Test] public void Hit_FeedbackRequested() { var a=Make(); ActivateChase(a); a.C.ReceiveDamage(new DamageInfo(new EntityId(1),10)); Assert.AreEqual(1,a.Feedback.Hits); }
        [TestCase(0.1f)] [TestCase(0.5f)] [TestCase(1f)] public void Definition_AcceptsValidDecisionIntervals(float interval) { Assert.DoesNotThrow(()=>DefWithDecision(interval)); }
        [TestCase(0f)] [TestCase(-1f)] public void Definition_RejectsInvalidMaxHealth(float hp) { Assert.Throws<ArgumentOutOfRangeException>(()=>new ZombieDefinition(hp,1,1,1,1,0,1,.1f,0,.1f,.1f,1,1,1)); }

        private static ZombieDefinition DefWithDecision(float i) => new ZombieDefinition(100,2,360,10,1.5f,.2f,1,i,0,.1f,.2f,1,1,1);
        private static ZombieSpawnRequest Req(float x,float y,float z) { var p=new ZombiePoint(x,y,z); return new ZombieSpawnRequest(in p); }
        private static void ActivateChase(Parts a) { var d=Def(); var r=Req(0,0,0); a.C.Activate(in d,in r); a.C.Tick(.01f); }
        private static void EnterAttack(Parts a) { ActivateChase(a); a.Target.Set(99,1,0,0); a.C.Tick(.01f); Assert.AreEqual(ZombieStateId.Attack,a.C.State); }

        private static Parts Make()
        {
            var ids=new Seq(); var view=new View(); var motor=new Motor(); var health=new Health(); var target=new Target(); var attack=new Attack(); var reg=new Reg(); var pool=new Pool(); var feedback=new Feedback(); var bus=new Bus();
            var factory=new ZombieFactory(ids,target,attack,feedback,bus);
            var c=factory.Create(view,motor,health,reg,pool);
            return new Parts(c,view,motor,health,target,attack,reg,pool,feedback,bus);
        }
        private sealed class Parts
        {
            public readonly ZombieController C; public readonly View View; public readonly Motor Motor; public readonly Health Health; public readonly Target Target; public readonly Attack Attack; public readonly Reg Reg; public readonly Pool Pool; public readonly Feedback Feedback; public readonly Bus Bus;
            public Parts(ZombieController c,View v,Motor m,Health h,Target t,Attack a,Reg r,Pool p,Feedback f,Bus b){C=c;View=v;Motor=m;Health=h;Target=t;Attack=a;Reg=r;Pool=p;Feedback=f;Bus=b;}
        }
        private sealed class Seq : IEntityIdGenerator { long n; public EntityId Next()=>new EntityId(++n); }
        private sealed class View : IZombieView { public ZombiePoint Position {get;set;}=new ZombiePoint(0,0,0); public bool Collision=true; public void ResetForReuse(){} public void SetActive(bool a){} public void SetLocomotionSpeed(float s){} public void SetGameplayCollisionEnabled(bool e)=>Collision=e; public void FaceTarget(in ZombiePoint t,float r,float d){} public void PlaySpawn(){} public void PlayAttack(){} public void PlayHit(){} public void PlayDeath(){} public void SetDissolveProgress(float p){} public void SetAnimationPaused(bool p){} }
        private sealed class Motor : IZombieMotor { public ZombiePoint Position {get;private set;}=new ZombiePoint(0,0,0); public float NormalizedSpeed=>1; public int MoveCount; public int StopCount; public void Warp(in ZombiePoint p){Position=p;} public void SetEnabled(bool e){} public void MoveTowards(in ZombiePoint t,float s,float d){MoveCount++;} public void Stop(){StopCount++;} }
        private sealed class Health : IZombieHealthPort { float hp,max; public bool IsAlive=>hp>0; public float CurrentHealth=>hp; public float MaxHealth=>max; public void Initialize(EntityId id,float m){max=m;hp=m;} public void ApplyDamage(DamageInfo d){hp=Math.Max(0,hp-d.Amount);} }
        private sealed class Target : IZombieTargetProvider { bool has; EntityId id; ZombiePoint p; public void Set(long i,float x,float y,float z){has=true;id=new EntityId(i);p=new ZombiePoint(x,y,z);} public void Clear()=>has=false; public bool TryAcquireTarget(in ZombiePoint z,out ZombieTarget t){if(!has){t=ZombieTarget.None;return false;} t=ZombieTarget.From(id,in p);return true;} public bool TryGetTarget(EntityId e,out ZombieTarget t){if(!has||!e.Equals(id)){t=ZombieTarget.None;return false;} t=ZombieTarget.From(id,in p);return true;} }
        private sealed class Attack : IZombieAttackPort { public int Count; public bool TryAttack(in ZombieAttackRequest r){Count++;return true;} }
        private sealed class Reg : IZombieTargetRegistrationPort { public int RegisterCount,UnregisterCount; public void Register(EntityId e)=>RegisterCount++; public void Unregister(EntityId e)=>UnregisterCount++; }
        private sealed class Pool : IZombiePoolReturnPort { public int ReturnCount; public void Return(EntityId e,ZombieReleaseReason r)=>ReturnCount++; }
        private sealed class Feedback : IZombieFeedbackPort { public int Hits,Deaths; public void OnHit(EntityId e,in ZombiePoint p)=>Hits++; public void OnDeath(EntityId e,in ZombiePoint p)=>Deaths++; }
        private sealed class Bus : IEventBus { public int Activated,Killed,Released; public EntityId LastKiller; public void Publish<T>(T evt) where T:IEvent { if(evt is ZombieActivatedEvent) Activated++; if(evt is ZombieKilledEvent k){Killed++;LastKiller=k.KillerId;} if(evt is ZombieReleasedEvent) Released++; } }
    }
}
