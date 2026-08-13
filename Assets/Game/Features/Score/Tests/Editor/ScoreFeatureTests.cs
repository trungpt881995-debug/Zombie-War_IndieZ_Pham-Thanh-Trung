using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using NUnit.Framework;
using ZombieWar.Features.Score.Catalog;
using ZombieWar.Features.Score.Controller;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Events;
using ZombieWar.Features.Score.Model;
using ZombieWar.Features.Score.Rules;
using ZombieWar.Features.Score.Services;

namespace ZombieWar.Features.Score.Tests
{
    public sealed class ScoreFeatureTests
    {
        private static ScoreRuleCatalog Catalog(long zombie = 10, long a = 100, long b = 200, long c = 300) =>
            new ScoreRuleCatalog(new[]
            {
                new ScoreRuleDefinition(ScoreActionId.NormalZombieDefeated, zombie),
                new ScoreRuleDefinition(ScoreActionId.BossADefeated, a),
                new ScoreRuleDefinition(ScoreActionId.BossBDefeated, b),
                new ScoreRuleDefinition(ScoreActionId.BossCDefeated, c)
            });

        private static ScoreRuntime Runtime(EventBus bus = null)
        {
            bus = bus ?? new EventBus();
            var runtime = new ScoreRuntime(bus);
            runtime.Initialize(Catalog());
            return runtime;
        }

        private static EntityId E(long v = 1) => new EntityId(v);

        [Test] public void Catalog_HasFourRequiredRules() => Assert.AreEqual(4, Catalog().Count);
        [Test] public void Catalog_DuplicateRejected() => Assert.Throws<ArgumentException>(() => new ScoreRuleCatalog(new[] { new ScoreRuleDefinition(ScoreActionId.NormalZombieDefeated,10), new ScoreRuleDefinition(ScoreActionId.NormalZombieDefeated,20), new ScoreRuleDefinition(ScoreActionId.BossADefeated,1), new ScoreRuleDefinition(ScoreActionId.BossBDefeated,1), new ScoreRuleDefinition(ScoreActionId.BossCDefeated,1) }));
        [Test] public void Catalog_MissingRequiredRejected() => Assert.Throws<ArgumentException>(() => new ScoreRuleCatalog(new[] { new ScoreRuleDefinition(ScoreActionId.NormalZombieDefeated,10) }));
        [Test] public void Definition_NoneRejected() => Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreRuleDefinition(ScoreActionId.None,10));
        [Test] public void Definition_ZeroPointsRejected() => Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreRuleDefinition(ScoreActionId.NormalZombieDefeated,0));
        [Test] public void FixedRule_ReturnsConfiguredPoints(){var r=new FixedScoreRule(ScoreActionId.BossADefeated,123);var c=new ScoreContext(ScoreActionId.BossADefeated,E(),ScoreLevelId.GameLevel01);Assert.AreEqual(123,r.Calculate(in c));}

        [Test] public void Runtime_InitialStateReady(){var r=Runtime();Assert.AreEqual(ScoreState.Ready,r.State);Assert.IsFalse(r.ScoringEnabled);}
        [Test] public void Runtime_StartRunResetsEverything(){var r=Runtime();r.StartRun();Assert.AreEqual(0,r.TotalScore);Assert.AreEqual(0,r.LevelScore);Assert.AreEqual(ScoreLevelId.None,r.CurrentLevel);Assert.IsTrue(r.ScoringEnabled);}
        [Test] public void BeginLevel_BeforeRunRejected(){var r=Runtime();Assert.IsFalse(r.BeginLevel(ScoreLevelId.GameLevel01));}
        [Test] public void BeginLevel_NoneRejected(){var r=Runtime();r.StartRun();Assert.IsFalse(r.BeginLevel(ScoreLevelId.None));}
        [Test] public void BeginLevel_UnknownRejected(){var r=Runtime();r.StartRun();Assert.IsFalse(r.BeginLevel((ScoreLevelId)99));}
        [Test] public void BeginLevel_SetsCheckpoint(){var r=Runtime();r.StartRun();Assert.IsTrue(r.BeginLevel(ScoreLevelId.GameLevel01));Assert.AreEqual(0,r.Snapshot.LevelStartTotalScore);}
        [Test] public void Award_BeforeLevelRejected(){var r=Runtime();r.StartRun();Assert.IsFalse(r.Award(ScoreActionId.NormalZombieDefeated,E()).Accepted);}
        [Test] public void Award_NormalZombieAddsTotalAndLevel(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);var x=r.Award(ScoreActionId.NormalZombieDefeated,E());Assert.IsTrue(x.Accepted);Assert.AreEqual(10,r.TotalScore);Assert.AreEqual(10,r.LevelScore);}
        [Test] public void Award_BossAUsesRule(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);Assert.AreEqual(100,r.Award(ScoreActionId.BossADefeated,E()).AwardedPoints);}
        [Test] public void Award_BossBUsesRule(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel02);Assert.AreEqual(200,r.Award(ScoreActionId.BossBDefeated,E()).AwardedPoints);}
        [Test] public void Award_BossCUsesRule(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel02);Assert.AreEqual(300,r.Award(ScoreActionId.BossCDefeated,E()).AwardedPoints);}
        [Test] public void Award_NoneRejected(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);Assert.IsFalse(r.Award(ScoreActionId.None,E()).Accepted);}
        [Test] public void Award_InvalidEntityRejected(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);Assert.IsFalse(r.Award(ScoreActionId.NormalZombieDefeated,E(0)).Accepted);}
        [Test] public void DisabledScoring_RejectsAward(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.SetScoringEnabled(false);Assert.IsFalse(r.Award(ScoreActionId.NormalZombieDefeated,E()).Accepted);Assert.AreEqual(0,r.TotalScore);}
        [Test] public void ReEnableScoring_AllowsAward(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.SetScoringEnabled(false);r.SetScoringEnabled(true);Assert.IsTrue(r.Award(ScoreActionId.NormalZombieDefeated,E()).Accepted);}
        [Test] public void BeginLevel2_CarriesTotalAndResetsLevel(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.BossADefeated,E());r.BeginLevel(ScoreLevelId.GameLevel02);Assert.AreEqual(100,r.TotalScore);Assert.AreEqual(0,r.LevelScore);Assert.AreEqual(100,r.Snapshot.LevelStartTotalScore);}
        [Test] public void ReplayLevel_RestoresCheckpoint(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.BossADefeated,E());r.BeginLevel(ScoreLevelId.GameLevel02);r.Award(ScoreActionId.BossBDefeated,E(2));Assert.AreEqual(300,r.TotalScore);Assert.IsTrue(r.ReplayCurrentLevel());Assert.AreEqual(100,r.TotalScore);Assert.AreEqual(0,r.LevelScore);}
        [Test] public void ReplayBeforeLevelRejected(){var r=Runtime();r.StartRun();Assert.IsFalse(r.ReplayCurrentLevel());}
        [Test] public void NewRunAfterScore_ResetsToZero(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.BossADefeated,E());r.StartRun();Assert.AreEqual(0,r.TotalScore);Assert.AreEqual(ScoreLevelId.None,r.CurrentLevel);}
        [Test] public void SnapshotReflectsRuntime(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.NormalZombieDefeated,E());var s=r.Snapshot;Assert.AreEqual(10,s.TotalScore);Assert.AreEqual(10,s.LevelScore);Assert.AreEqual(ScoreLevelId.GameLevel01,s.CurrentLevel);}

        [Test] public void StartRunEvent_FiresOnce(){var b=new EventBus();int n=0;b.Subscribe<ScoreRunStartedEvent>(_=>n++);var r=Runtime(b);r.StartRun();Assert.AreEqual(1,n);}
        [Test] public void LevelStartedEvent_FiresOnce(){var b=new EventBus();int n=0;b.Subscribe<ScoreLevelStartedEvent>(_=>n++);var r=Runtime(b);r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);Assert.AreEqual(1,n);}
        [Test] public void AcceptedAward_ChangedEventOnce(){var b=new EventBus();int n=0;b.Subscribe<ScoreChangedEvent>(_=>n++);var r=Runtime(b);r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.NormalZombieDefeated,E());Assert.AreEqual(1,n);}
        [Test] public void RejectedAward_NoChangedEvent(){var b=new EventBus();int n=0;b.Subscribe<ScoreChangedEvent>(_=>n++);var r=Runtime(b);r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.SetScoringEnabled(false);r.Award(ScoreActionId.NormalZombieDefeated,E());Assert.AreEqual(0,n);}
        [Test] public void ChangedEvent_HasCorrectDelta(){var b=new EventBus();ScoreChangedEvent last=default;b.Subscribe<ScoreChangedEvent>(e=>last=e);var r=Runtime(b);r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.BossADefeated,E(44));Assert.AreEqual(100,last.Delta);Assert.AreEqual(100,last.CurrentTotal);Assert.AreEqual(ScoreActionId.BossADefeated,last.ActionId);Assert.AreEqual(E(44),last.SourceEntityId);}
        [Test] public void ReplayEvent_FiresOnce(){var b=new EventBus();int n=0;b.Subscribe<ScoreLevelReplayedEvent>(_=>n++);var r=Runtime(b);r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.ReplayCurrentLevel();Assert.AreEqual(1,n);}
        [Test] public void EnabledChangedEvent_NoDuplicateForSameValue(){var b=new EventBus();int n=0;b.Subscribe<ScoringEnabledChangedEvent>(_=>n++);var r=Runtime(b);r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.SetScoringEnabled(true);r.SetScoringEnabled(false);r.SetScoringEnabled(false);Assert.AreEqual(1,n);}

        [Test] public void MultipleZombieAwardsAccumulate(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);for(int i=1;i<=25;i++)r.Award(ScoreActionId.NormalZombieDefeated,E(i));Assert.AreEqual(250,r.TotalScore);}
        [Test] public void BossBCOrderIndependent(){var r1=Runtime();r1.StartRun();r1.BeginLevel(ScoreLevelId.GameLevel02);r1.Award(ScoreActionId.BossBDefeated,E(1));r1.Award(ScoreActionId.BossCDefeated,E(2));var r2=Runtime();r2.StartRun();r2.BeginLevel(ScoreLevelId.GameLevel02);r2.Award(ScoreActionId.BossCDefeated,E(2));r2.Award(ScoreActionId.BossBDefeated,E(1));Assert.AreEqual(r1.TotalScore,r2.TotalScore);}
        [Test] public void LevelCheckpointNotChangedByAwards(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.Award(ScoreActionId.NormalZombieDefeated,E());Assert.AreEqual(0,r.Snapshot.LevelStartTotalScore);}
        [Test] public void ReplayReEnablesScoring(){var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);r.SetScoringEnabled(false);r.ReplayCurrentLevel();Assert.IsTrue(r.ScoringEnabled);}
        [Test] public void BeginLevelReEnablesScoring(){var r=Runtime();r.StartRun();r.SetScoringEnabled(false);r.BeginLevel(ScoreLevelId.GameLevel01);Assert.IsTrue(r.ScoringEnabled);}
        [Test] public void Shutdown_ReturnsUninitialized(){var r=Runtime();((IScoreRuntimeConfigurator)r).Shutdown();Assert.IsFalse(r.IsInitialized);Assert.AreEqual(ScoreState.Uninitialized,r.State);}
        [Test] public void StartRunBeforeInitialize_NoThrow(){var r=new ScoreRuntime(new EventBus());Assert.DoesNotThrow(()=>r.StartRun());}
        [Test] public void AwardBeforeInitializeRejected(){var r=new ScoreRuntime(new EventBus());Assert.IsFalse(r.Award(ScoreActionId.NormalZombieDefeated,E()).Accepted);}
        [Test] public void InitializeTwiceRejected(){var r=Runtime();Assert.Throws<InvalidOperationException>(()=>((IScoreRuntimeConfigurator)r).Initialize(Catalog()));}

        [TestCase(ScoreActionId.NormalZombieDefeated,10)]
        [TestCase(ScoreActionId.BossADefeated,100)]
        [TestCase(ScoreActionId.BossBDefeated,200)]
        [TestCase(ScoreActionId.BossCDefeated,300)]
        public void RuleValues_AreDataDriven(ScoreActionId action,long expected)
        {
            var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);Assert.AreEqual(expected,r.Award(action,E()).AwardedPoints);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        [TestCase(2500)]
        public void ZombieAward_CountScalesLinearly(int count)
        {
            var r=Runtime();r.StartRun();r.BeginLevel(ScoreLevelId.GameLevel01);for(int i=1;i<=count;i++)r.Award(ScoreActionId.NormalZombieDefeated,E(i));Assert.AreEqual(count*10L,r.TotalScore);
        }

        [Test] public void Overflow_TotalRejectsWithoutMutation()
        {
            var bus=new EventBus();
            var model=new ScoreModel();
            var catalog=Catalog(long.MaxValue,1,1,1);
            var controller=new ScoreController(model,catalog,bus);
            controller.Initialize();controller.StartRun();controller.BeginLevel(ScoreLevelId.GameLevel01);
            var c1=new ScoreContext(ScoreActionId.NormalZombieDefeated,E(1),ScoreLevelId.GameLevel01);
            Assert.IsTrue(controller.Award(in c1).Accepted);
            var c2=new ScoreContext(ScoreActionId.NormalZombieDefeated,E(2),ScoreLevelId.GameLevel01);
            Assert.IsFalse(controller.Award(in c2).Accepted);
            Assert.AreEqual(long.MaxValue,controller.Snapshot().TotalScore);
        }
    }
}
