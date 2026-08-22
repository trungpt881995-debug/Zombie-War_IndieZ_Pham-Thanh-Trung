using System;
using System.Collections.Generic;
using NUnit.Framework;
using GeneralCore.Architecture;
using ZombieWar.Features.Level.Catalog;
using ZombieWar.Features.Level.Domain;
using ZombieWar.Features.Level.Events;
using ZombieWar.Features.Level.Services;
namespace ZombieWar.Features.Level.Tests
{
    public sealed class LevelFeatureTests
    {
        private static LevelDefinition Def(GameLevelId id, bool final, LevelBossObjectiveId bosses) => new LevelDefinition(id, final, new[]
        {
            new SoldierProgressionStep(SoldierGroupLevelId.Level1, 0), new SoldierProgressionStep(SoldierGroupLevelId.Level2, 200), new SoldierProgressionStep(SoldierGroupLevelId.Level3, 700), new SoldierProgressionStep(SoldierGroupLevelId.Level4, 1500)
        }
        , 2500, bosses);
        private static LevelRuntime Runtime(EventBus bus=null)
        {
            bus??=new EventBus();
            var r=new LevelRuntime(bus);
            r.Initialize(new LevelCatalog(new[]
            {
                Def(GameLevelId.GameLevel01, false, LevelBossObjectiveId.BossA), Def(GameLevelId.GameLevel02, true, LevelBossObjectiveId.BossB|LevelBossObjectiveId.BossC)
            }
            ));
            return r;
        }
        [Test] public void Initial_Is_Uninitialized()
        {
            var r=new LevelRuntime(new EventBus());
            Assert.AreEqual(LevelState.Uninitialized, r.State);
        }
        [Test] public void Initialize_Is_Ready()
        {
            var r=Runtime();
            Assert.AreEqual(LevelState.Ready, r.State);
        }
        [Test] public void Begin_GL1_Running()
        {
            var r=Runtime();
            Assert.True(r.BeginLevel(GameLevelId.GameLevel01));
            Assert.AreEqual(LevelState.Running, r.State);
        }
        [Test] public void Begin_Starts_Level1()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.AreEqual(SoldierGroupLevelId.Level1, r.SoldierGroupLevel);
        }
        [Test] public void Begin_Starts_Zero_Kills()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.AreEqual(0, r.NormalZombieKillCount);
        }
        [Test] public void Begin_Starts_NormalCombat()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.AreEqual(LevelPhase.NormalCombat, r.Phase);
        }
        [Test] public void Unknown_Level_Rejected()
        {
            var r=Runtime();
            Assert.False(r.BeginLevel((GameLevelId)99));
        }
        [Test] public void NonPositive_Bulk_Rejected()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.False(r.RegisterNormalZombieKills(0));
            Assert.False(r.RegisterNormalZombieKills(-1));
        }
        [TestCase(199, SoldierGroupLevelId.Level1)]
        [TestCase(200, SoldierGroupLevelId.Level2)]
        [TestCase(699, SoldierGroupLevelId.Level2)]
        [TestCase(700, SoldierGroupLevelId.Level3)]
        [TestCase(1499, SoldierGroupLevelId.Level3)]
        [TestCase(1500, SoldierGroupLevelId.Level4)]
        public void Soldier_Thresholds(int kills, SoldierGroupLevelId expected)
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(kills);
            Assert.AreEqual(expected, r.SoldierGroupLevel);
        }
        [TestCase(2499, LevelPhase.NormalCombat)]
        [TestCase(2500, LevelPhase.BossPhase)]
        [TestCase(2600, LevelPhase.BossPhase)] public void Boss_Threshold(int kills, LevelPhase expected)
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(kills);
            Assert.AreEqual(expected, r.Phase);
        }
        [Test] public void Bulk_Crosses_All_Levels()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(1600);
            Assert.AreEqual(SoldierGroupLevelId.Level4, r.SoldierGroupLevel);
        }
        [Test] public void Bulk_Event_Order()
        {
            var b=new EventBus();
            var order=new List<SoldierGroupLevelId>();
            b.Subscribe<SoldierGroupLevelChangedEvent>(e => order.Add(e.Current));
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(1600);
            CollectionAssert.AreEqual(new[]
            {
                SoldierGroupLevelId.Level2, SoldierGroupLevelId.Level3, SoldierGroupLevelId.Level4
            }
            , order);
        }
        [Test] public void BossPhase_Fires_Once()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<BossPhaseStartedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterNormalZombieKill();
            Assert.AreEqual(1, n);
        }
        [Test] public void LevelUp_Fires_Once()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<SoldierGroupLevelChangedEvent>(e=>
            {
                if (e.Current == SoldierGroupLevelId.Level2)n++;
            }
            );
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(300);
            Assert.AreEqual(1, n);
        }
        [Test] public void Disabled_Ignores_Kill()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.SetProgressionEnabled(false);
            Assert.False(r.RegisterNormalZombieKill());
            Assert.AreEqual(0, r.NormalZombieKillCount);
        }
        [Test] public void Resume_Accepts_Kill()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.SetProgressionEnabled(false);
            r.SetProgressionEnabled(true);
            Assert.True(r.RegisterNormalZombieKill());
        }
        [Test] public void Boss_Before_Phase_Rejected()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.False(r.RegisterBossDefeated(LevelBossObjectiveId.BossA));
        }
        [Test] public void GL1_Wrong_Boss_Rejected()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            Assert.False(r.RegisterBossDefeated(LevelBossObjectiveId.BossB));
        }
        [Test] public void GL1_BossA_Completes()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            Assert.True(r.RegisterBossDefeated(LevelBossObjectiveId.BossA));
            Assert.AreEqual(LevelState.Completed, r.State);
        }
        [Test] public void GL1_Does_Not_Emit_GameCompleted()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<GameCompletedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossA);
            Assert.AreEqual(0, n);
        }
        [Test] public void Duplicate_Boss_Ignored()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel02);
            r.RegisterNormalZombieKills(2500);
            Assert.True(r.RegisterBossDefeated(LevelBossObjectiveId.BossB));
            Assert.False(r.RegisterBossDefeated(LevelBossObjectiveId.BossB));
        }
        [Test] public void GL2_BossB_Alone_Not_Complete()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel02);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossB);
            Assert.AreEqual(LevelState.Running, r.State);
        }
        [Test] public void GL2_BossB_C_Complete()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel02);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossB);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossC);
            Assert.AreEqual(LevelState.Completed, r.State);
        }
        [Test] public void GL2_Emits_GameCompleted()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<GameCompletedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel02);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossB);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossC);
            Assert.AreEqual(1, n);
        }
        [Test] public void Replay_Resets()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(1000);
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.AreEqual(0, r.NormalZombieKillCount);
            Assert.AreEqual(SoldierGroupLevelId.Level1, r.SoldierGroupLevel);
        }
        [Test] public void GL1_To_GL2_Resets()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.BeginLevel(GameLevelId.GameLevel02);
            Assert.AreEqual(0, r.NormalZombieKillCount);
            Assert.AreEqual(LevelPhase.NormalCombat, r.Phase);
        }
        [Test] public void Completed_Ignores_Kills()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossA);
            Assert.False(r.RegisterNormalZombieKill());
        }
        [Test] public void Snapshot_Next_Threshold_Lv1()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.AreEqual(200, r.Snapshot().NextThreshold);
        }
        [Test] public void Snapshot_Next_Threshold_Lv2()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(200);
            Assert.AreEqual(700, r.Snapshot().NextThreshold);
        }
        [Test] public void Snapshot_Boss_Threshold_After_Lv4()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(1500);
            Assert.AreEqual(2500, r.Snapshot().NextThreshold);
        }
        [Test] public void Catalog_Duplicate_Rejected()
        {
            Assert.Throws<ArgumentException>(() => new LevelCatalog(new[]
            {
                Def(GameLevelId.GameLevel01, false, LevelBossObjectiveId.BossA), Def(GameLevelId.GameLevel01, true, LevelBossObjectiveId.BossB)
            }
            ));
        }
        [Test] public void Catalog_Requires_One_Final()
        {
            Assert.Throws<ArgumentException>(() => new LevelCatalog(new[]
            {
                Def(GameLevelId.GameLevel01, false, LevelBossObjectiveId.BossA)
            }
            ));
        }
        [Test] public void Definition_Requires_Four_Steps()
        {
            Assert.Throws<ArgumentException>(() => new LevelDefinition(GameLevelId.GameLevel01, false, new[]
            {
                new SoldierProgressionStep(SoldierGroupLevelId.Level1, 0)
            }
            , 2500, LevelBossObjectiveId.BossA));
        }
        [Test] public void Definition_Rejects_None_Boss()
        {
            Assert.Throws<ArgumentException>(() => new LevelDefinition(GameLevelId.GameLevel01, false, new[]
            {
                new SoldierProgressionStep(SoldierGroupLevelId.Level1, 0), new SoldierProgressionStep(SoldierGroupLevelId.Level2, 200), new SoldierProgressionStep(SoldierGroupLevelId.Level3, 700), new SoldierProgressionStep(SoldierGroupLevelId.Level4, 1500)
            }
            , 2500, LevelBossObjectiveId.None));
        }
        [Test] public void Shutdown_Resets()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            ((ILevelRuntimeConfigurator)r).Shutdown();
            Assert.False(r.IsInitialized);
            Assert.AreEqual(LevelState.Uninitialized, r.State);
        }
        [Test] public void Start_Event_Once_Per_Begin()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<GameLevelStartedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            Assert.AreEqual(1, n);
        }
        [Test] public void Progress_Event_On_Kill()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<LevelKillProgressChangedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            int before=n;
            r.RegisterNormalZombieKill();
            Assert.AreEqual(before+1, n);
        }
        [Test] public void Boss_Objective_Event_Once()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<BossObjectiveCompletedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossA);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossA);
            Assert.AreEqual(1, n);
        }
        [Test] public void GameLevelCompleted_Event_Once()
        {
            var b=new EventBus();
            int n=0;
            b.Subscribe<GameLevelCompletedEvent>(e => n++);
            var r=Runtime(b);
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossA);
            Assert.AreEqual(1, n);
        }
        [TestCase(GameLevelId.GameLevel01, false)]
        [TestCase(GameLevelId.GameLevel02, true)] public void Catalog_Final_Flag(GameLevelId id, bool expected)
        {
            var c=new LevelCatalog(new[]
            {
                Def(GameLevelId.GameLevel01, false, LevelBossObjectiveId.BossA), Def(GameLevelId.GameLevel02, true, LevelBossObjectiveId.BossB|LevelBossObjectiveId.BossC)
            }
            );
            Assert.True(c.TryGet(id, out var d));
            Assert.AreEqual(expected, d.IsFinalLevel);
        }
        [TestCase(LevelBossObjectiveId.None)]
        [TestCase((LevelBossObjectiveId)8)] public void Invalid_Boss_Rejected(LevelBossObjectiveId boss)
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            Assert.False(r.RegisterBossDefeated(boss));
        }
        [Test] public void Kills_Continue_After_BossPhase()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterNormalZombieKill();
            Assert.AreEqual(2501, r.NormalZombieKillCount);
        }
        [Test] public void Progression_Does_Not_Level_After_BossPhase()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            var level=r.SoldierGroupLevel;
            r.RegisterNormalZombieKills(100);
            Assert.AreEqual(level, r.SoldierGroupLevel);
        }
        [Test] public void SetEnabled_After_Completed_Remains_False()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel01);
            r.RegisterNormalZombieKills(2500);
            r.RegisterBossDefeated(LevelBossObjectiveId.BossA);
            r.SetProgressionEnabled(true);
            Assert.False(r.ProgressionEnabled);
        }
        [Test] public void Register_MultiFlag_Boss_Rejected()
        {
            var r=Runtime();
            r.BeginLevel(GameLevelId.GameLevel02);
            r.RegisterNormalZombieKills(2500);
            Assert.False(r.RegisterBossDefeated(LevelBossObjectiveId.BossB|LevelBossObjectiveId.BossC));
        }
    }
}
