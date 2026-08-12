using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using NUnit.Framework;
using ZombieWar.Features.Control.Domain;
using ZombieWar.Features.Soldier.Controller;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Events;
using ZombieWar.Features.Soldier.Factories;
using ZombieWar.Features.Soldier.Formation;
using ZombieWar.Features.Soldier.Model;
using ZombieWar.Features.Soldier.Movement;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Soldier.View;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Factories;
using ZombieWar.Features.Targeting.View;
using ZombieWar.Integration.Soldier;

namespace ZombieWar.Features.Soldier.Tests
{
    public sealed class SoldierFeatureTests
    {
        [Test]
        public void GroupModel_StartsAtLevel1()
        {
            var model =
                new SoldierGroupModel(
                    new EntityId(100));

            Assert.AreEqual(
                SoldierGroupLevel.Level1,
                model.Level);

            Assert.AreEqual(
                1,
                model.RequiredSoldierCount);

            Assert.IsTrue(
                model.GameplayEnabled);

            Assert.IsFalse(
                model.MoveInput.HasInput);
        }

        [Test]
        public void GroupModel_AdvancesSequentiallyOnly()
        {
            var model =
                new SoldierGroupModel(
                    new EntityId(1));

            Assert.IsTrue(
                model.TryAdvanceTo(
                    SoldierGroupLevel.Level2));

            Assert.IsTrue(
                model.TryAdvanceTo(
                    SoldierGroupLevel.Level3));

            Assert.IsTrue(
                model.TryAdvanceTo(
                    SoldierGroupLevel.Level4));

            Assert.IsFalse(
                model.TryAdvanceTo(
                    SoldierGroupLevel.Level4));
        }

        [Test]
        public void GroupModel_RejectsSkippedAndBackwardLevel()
        {
            var model =
                new SoldierGroupModel(
                    new EntityId(1));

            Assert.IsFalse(
                model.TryAdvanceTo(
                    SoldierGroupLevel.Level3));

            Assert.AreEqual(
                SoldierGroupLevel.Level1,
                model.Level);
        }

        [Test]
        public void GroupModel_DisableClearsMoveInput()
        {
            var model =
                new SoldierGroupModel(
                    new EntityId(1));

            var input =
                new SoldierMoveInput(
                    1f,
                    0f,
                    1f);

            model.SetMoveInput(
                in input);

            model.SetGameplayEnabled(false);

            Assert.IsFalse(
                model.GameplayEnabled);

            Assert.IsFalse(
                model.MoveInput.HasInput);
        }

        [Test]
        public void GroupModel_ResetReturnsLevel1AndZeroInput()
        {
            var model =
                new SoldierGroupModel(
                    new EntityId(1));

            model.TryAdvanceTo(
                SoldierGroupLevel.Level2);

            var input =
                new SoldierMoveInput(
                    1f,
                    0f,
                    1f);

            model.SetMoveInput(
                in input);

            model.Reset();

            Assert.AreEqual(
                SoldierGroupLevel.Level1,
                model.Level);

            Assert.IsFalse(
                model.MoveInput.HasInput);

            Assert.IsTrue(
                model.GameplayEnabled);
        }

        [TestCase(-0.01f)]
        [TestCase(1.01f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void MoveInput_InvalidMagnitudeThrows(
            float magnitude)
        {
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () =>
                {
                    new SoldierMoveInput(
                        0f,
                        0f,
                        magnitude);
                });
        }

        [Test]
        public void MovementSolver_ZeroInputProducesZeroVelocity()
        {
            var solver =
                new SoldierMovementSolver();

            SoldierMoveInput input =
                SoldierMoveInput.Zero;

            SoldierMovementStep result =
                solver.Solve(
                    in input,
                    5f);

            Assert.AreEqual(
                0f,
                result.VelocityX);

            Assert.AreEqual(
                0f,
                result.VelocityZ);

            Assert.AreEqual(
                0f,
                result.NormalizedSpeed);
        }

        [Test]
        public void MovementSolver_ScalesVelocityFromInputVector()
        {
            var solver =
                new SoldierMovementSolver();

            var input =
                new SoldierMoveInput(
                    0.5f,
                    -0.25f,
                    0.6f);

            SoldierMovementStep result =
                solver.Solve(
                    in input,
                    10f);

            Assert.AreEqual(
                5f,
                result.VelocityX,
                0.0001f);

            Assert.AreEqual(
                -2.5f,
                result.VelocityZ,
                0.0001f);

            Assert.AreEqual(
                0.6f,
                result.NormalizedSpeed,
                0.0001f);
        }

        [Test]
        public void FormationLayout_RequiresOfficialSoldierCount()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    new FormationLayout(
                        SoldierGroupLevel.Level3,
                        new[]
                        {
                            Slot(0f, 0f),
                            Slot(1f, 0f)
                        });
                });
        }

        [Test]
        public void FormationProvider_ReturnsCorrectLayouts()
        {
            IFormationProvider provider =
                CreateFormationProvider();

            Assert.AreEqual(
                1,
                provider.Get(
                    SoldierGroupLevel.Level1).Count);

            Assert.AreEqual(
                2,
                provider.Get(
                    SoldierGroupLevel.Level2).Count);

            Assert.AreEqual(
                3,
                provider.Get(
                    SoldierGroupLevel.Level3).Count);

            Assert.AreEqual(
                4,
                provider.Get(
                    SoldierGroupLevel.Level4).Count);
        }

        [Test]
        public void SoldierController_InactiveDoesNothing()
        {
            var targeting =
                new FakeTargetingPort();

            var attack =
                new FakeAttackPort();

            var view =
                new FakeSoldierView();

            SoldierController controller =
                CreateSoldierController(
                    10,
                    view,
                    targeting,
                    attack);

            controller.Tick(
                10f,
                1f,
                0.016f);

            Assert.AreEqual(
                0,
                targeting.EvaluateCount);

            Assert.AreEqual(
                0,
                attack.UpdateCount);
        }

        [Test]
        public void SoldierController_NoTargetClearsAimAndAttack()
        {
            var targeting =
                new FakeTargetingPort
                {
                    Result =
                        SoldierTargetInfo.None
                };

            var attack =
                new FakeAttackPort();

            var view =
                new FakeSoldierView
                {
                    WorldPosition =
                        new SoldierPoint(
                            0f,
                            0f,
                            0f)
                };

            SoldierController controller =
                CreateSoldierController(
                    10,
                    view,
                    targeting,
                    attack);

            SoldierPoint slot =
                SoldierPoint.Zero;

            controller.Activate(
                0,
                in slot);

            controller.Tick(
                10f,
                0.75f,
                0.016f);

            Assert.AreEqual(
                1,
                targeting.EvaluateCount);

            Assert.AreEqual(
                1,
                attack.ClearCount);

            // Activate() clears stale presentation once; the no-target Tick
            // clears it again.
            Assert.AreEqual(
                2,
                view.ClearAimCount);

            Assert.AreEqual(
                0.75f,
                view.LastMovementSpeed,
                0.0001f);
        }

        [Test]
        public void SoldierController_ValidTargetAimsAndAttacks()
        {
            var targetPosition =
                new SoldierPoint(
                    10f,
                    0f,
                    0f);

            var targeting =
                new FakeTargetingPort
                {
                    Result =
                        SoldierTargetInfo.From(
                            new EntityId(99),
                            in targetPosition)
                };

            var attack =
                new FakeAttackPort();

            var view =
                new FakeSoldierView
                {
                    WorldPosition =
                        SoldierPoint.Zero
                };

            SoldierController controller =
                CreateSoldierController(
                    10,
                    view,
                    targeting,
                    attack);

            SoldierPoint slot =
                SoldierPoint.Zero;

            controller.Activate(
                0,
                in slot);

            controller.Tick(
                20f,
                0.5f,
                0.016f);

            Assert.AreEqual(
                1,
                view.AimCount);

            Assert.AreEqual(
                1f,
                view.LastAim.X,
                0.0001f);

            Assert.AreEqual(
                0f,
                view.LastAim.Z,
                0.0001f);

            Assert.AreEqual(
                1,
                attack.UpdateCount);

            Assert.AreEqual(
                99,
                attack.LastTarget.TargetId.Value);
        }

        [Test]
        public void SoldierController_DeactivateClearsTargetAndAttack()
        {
            var targeting =
                new FakeTargetingPort();

            var attack =
                new FakeAttackPort();

            var view =
                new FakeSoldierView();

            SoldierController controller =
                CreateSoldierController(
                    10,
                    view,
                    targeting,
                    attack);

            SoldierPoint slot =
                SoldierPoint.Zero;

            controller.Activate(
                0,
                in slot);

            controller.Deactivate();

            Assert.IsFalse(
                controller.Active);

            Assert.AreEqual(
                1,
                targeting.ClearCount);

            Assert.AreEqual(
                1,
                attack.ClearCount);

            Assert.IsFalse(
                view.Active);
        }

        [Test]
        public void GroupController_InitializesOnlyFirstSoldier()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            Assert.AreEqual(
                1,
                ctx.Controller.ActiveSoldierCount);

            Assert.IsTrue(
                ctx.Views[0].Active);

            Assert.IsFalse(
                ctx.Views[1].Active);

            Assert.IsFalse(
                ctx.Views[2].Active);

            Assert.IsFalse(
                ctx.Views[3].Active);
        }

        [Test]
        public void GroupController_Level2ActivatesSecondAndRepositionsFirst()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            Assert.IsTrue(
                ctx.Controller.TryAdvanceTo(
                    SoldierGroupLevel.Level2));

            Assert.AreEqual(
                2,
                ctx.Controller.ActiveSoldierCount);

            Assert.IsTrue(
                ctx.Views[1].Active);

            Assert.AreEqual(
                -1f,
                ctx.Views[0].LocalFormationPosition.X,
                0.0001f);

            Assert.AreEqual(
                1f,
                ctx.Views[1].LocalFormationPosition.X,
                0.0001f);
        }

        [Test]
        public void GroupController_CanReachLevel4WithFourActiveSoldiers()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            ctx.Controller.TryAdvanceTo(
                SoldierGroupLevel.Level2);

            ctx.Controller.TryAdvanceTo(
                SoldierGroupLevel.Level3);

            ctx.Controller.TryAdvanceTo(
                SoldierGroupLevel.Level4);

            Assert.AreEqual(
                4,
                ctx.Controller.ActiveSoldierCount);

            for (int i = 0;
                 i < ctx.Views.Length;
                 i++)
            {
                Assert.IsTrue(
                    ctx.Views[i].Active);
            }
        }

        [Test]
        public void GroupController_RejectsSkippedLevel()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            Assert.IsFalse(
                ctx.Controller.TryAdvanceTo(
                    SoldierGroupLevel.Level3));

            Assert.AreEqual(
                SoldierGroupLevel.Level1,
                ctx.Controller.Level);
        }

        [Test]
        public void GroupController_ResetReturnsToOneSoldier()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            ctx.Controller.TryAdvanceTo(
                SoldierGroupLevel.Level2);

            ctx.Controller.TryAdvanceTo(
                SoldierGroupLevel.Level3);

            ctx.Controller.ResetForGameLevel();

            Assert.AreEqual(
                SoldierGroupLevel.Level1,
                ctx.Controller.Level);

            Assert.AreEqual(
                1,
                ctx.Controller.ActiveSoldierCount);

            Assert.IsTrue(
                ctx.Views[0].Active);

            Assert.IsFalse(
                ctx.Views[1].Active);

            Assert.IsFalse(
                ctx.Views[2].Active);
        }

        [Test]
        public void GroupController_DisableClearsInputAndStopsActiveSoldiers()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            var input =
                new SoldierMoveInput(
                    1f,
                    0f,
                    1f);

            ctx.Input.Set(
                in input);

            ctx.Controller.SetGameplayEnabled(
                false);

            Assert.IsFalse(
                ctx.Controller.GameplayEnabled);

            Assert.IsFalse(
                ctx.Input.Current.HasInput);

            Assert.GreaterOrEqual(
                ctx.Targeting.ClearCount,
                1);

            Assert.GreaterOrEqual(
                ctx.Attack.ClearCount,
                1);
        }

        [Test]
        public void GroupController_TickUsesLatestInputAndTargetRange()
        {
            GroupTestContext ctx =
                CreateGroupContext();

            var input =
                new SoldierMoveInput(
                    1f,
                    0f,
                    1f);

            ctx.Input.Set(
                in input);

            ctx.Range.Range = 12f;

            ctx.Controller.Tick(
                0.02f);

            Assert.AreEqual(
                1,
                ctx.GroupView.MoveCount);

            Assert.AreEqual(
                5f,
                ctx.GroupView.LastMovement.VelocityX,
                0.0001f);

            Assert.AreEqual(
                12f,
                ctx.Targeting.LastRange,
                0.0001f);
        }

        [Test]
        public void GroupController_PublishesLevelChangedExactlyOnce()
        {
            var bus =
                new EventBus();

            int count = 0;

            IDisposable subscription =
                bus.Subscribe<
                    SoldierGroupLevelChangedEvent>(
                    _ => count++);

            try
            {
                GroupTestContext ctx =
                    CreateGroupContext(bus);

                ctx.Controller.TryAdvanceTo(
                    SoldierGroupLevel.Level2);

                Assert.AreEqual(
                    1,
                    count);
            }
            finally
            {
                subscription.Dispose();
            }
        }

        [Test]
        public void GroupController_PublishesSoldierAddedForInitialAndLevelUp()
        {
            var bus =
                new EventBus();

            int count = 0;

            IDisposable subscription =
                bus.Subscribe<
                    SoldierAddedEvent>(
                    _ => count++);

            try
            {
                GroupTestContext ctx =
                    CreateGroupContext(bus);

                Assert.AreEqual(
                    1,
                    count);

                ctx.Controller.TryAdvanceTo(
                    SoldierGroupLevel.Level2);

                Assert.AreEqual(
                    2,
                    count);
            }
            finally
            {
                subscription.Dispose();
            }
        }

        [Test]
        public void SoldierFactory_CreatesUniqueEntityIds()
        {
            var factory =
                new SoldierFactory(
                    new SequentialEntityIdGenerator(100),
                    NullSoldierTargetingPort.Instance,
                    NullSoldierAttackPort.Instance);

            SoldierSettings settings =
                DefaultSettings();

            SoldierController a =
                factory.Create(
                    0,
                    new FakeSoldierView(),
                    in settings);

            SoldierController b =
                factory.Create(
                    1,
                    new FakeSoldierView(),
                    in settings);

            Assert.AreEqual(
                100,
                a.EntityId.Value);

            Assert.AreEqual(
                101,
                b.EntityId.Value);
        }

        [Test]
        public void GroupFactory_CreatesGroupWithIndependentGroupId()
        {
            var bus =
                new EventBus();

            var ids =
                new SequentialEntityIdGenerator(1);

            var targeting =
                new FakeTargetingPort();

            var attack =
                new FakeAttackPort();

            var soldierFactory =
                new SoldierFactory(
                    ids,
                    targeting,
                    attack);

            var groupFactory =
                new SoldierGroupFactory(
                    ids,
                    soldierFactory,
                    new SoldierMovementSolver(),
                    new SoldierGroupInputBuffer(),
                    new FakeRangeProvider(),
                    bus);

            var views =
                new ISoldierView[]
                {
                    new FakeSoldierView(),
                    new FakeSoldierView(),
                    new FakeSoldierView(),
                    new FakeSoldierView()
                };

            SoldierSettings settings =
                DefaultSettings();

            SoldierGroupController group =
                groupFactory.Create(
                    new FakeGroupView(),
                    views,
                    in settings,
                    CreateFormationProvider());

            // Group identity is allocated first, before the four Soldier IDs.
            Assert.AreEqual(
                1,
                group.GroupId.Value);
        }

        [Test]
        public void ControlAdapter_ConvertsMovementIntentToSoldierInput()
        {
            var buffer =
                new SoldierGroupInputBuffer();

            var adapter =
                new ControlMovementIntentToSoldierAdapter(
                    buffer);

            var intent =
                new MovementIntent(
                    0.4f,
                    -0.7f,
                    0.8f);

            adapter.Set(
                in intent);

            Assert.AreEqual(
                0.4f,
                buffer.Current.X,
                0.0001f);

            Assert.AreEqual(
                -0.7f,
                buffer.Current.Y,
                0.0001f);

            Assert.AreEqual(
                0.8f,
                buffer.Current.Magnitude,
                0.0001f);
        }

        [Test]
        public void TargetingAdapter_CreatesIndependentSessionPerSoldier()
        {
            var factory =
                new FakeTargetingFactory();

            using (var adapter =
                new TargetingToSoldierAdapter(
                    factory))
            {
                SoldierPoint p =
                    SoldierPoint.Zero;

                adapter.Evaluate(
                    new EntityId(10),
                    in p,
                    10f);

                adapter.Evaluate(
                    new EntityId(20),
                    in p,
                    10f);

                adapter.Evaluate(
                    new EntityId(10),
                    in p,
                    10f);

                Assert.AreEqual(
                    2,
                    factory.CreateCount);

                Assert.AreEqual(
                    2,
                    factory.Sessions.Count);

                Assert.AreEqual(
                    2,
                    factory.Sessions[10].EvaluateCount);

                Assert.AreEqual(
                    1,
                    factory.Sessions[20].EvaluateCount);
            }
        }

        private static SoldierController CreateSoldierController(
            long id,
            FakeSoldierView view,
            FakeTargetingPort targeting,
            FakeAttackPort attack)
        {
            SoldierSettings settings =
                DefaultSettings();

            return new SoldierController(
                new SoldierModel(
                    new EntityId(id)),
                view,
                targeting,
                attack,
                in settings);
        }

        private static GroupTestContext CreateGroupContext(
            EventBus bus = null)
        {
            bus = bus ?? new EventBus();

            var targeting =
                new FakeTargetingPort();

            var attack =
                new FakeAttackPort();

            SoldierSettings settings =
                DefaultSettings();

            var soldiers =
                new SoldierController[4];

            var views =
                new FakeSoldierView[4];

            for (int i = 0; i < 4; i++)
            {
                views[i] =
                    new FakeSoldierView();

                soldiers[i] =
                    new SoldierController(
                        new SoldierModel(
                            new EntityId(i + 1)),
                        views[i],
                        targeting,
                        attack,
                        in settings);
            }

            var groupView =
                new FakeGroupView();

            var input =
                new SoldierGroupInputBuffer();

            var range =
                new FakeRangeProvider
                {
                    Range = 10f
                };

            var controller =
                new SoldierGroupController(
                    new SoldierGroupModel(
                        new EntityId(1000)),
                    groupView,
                    soldiers,
                    new SoldierMovementSolver(),
                    input,
                    CreateFormationProvider(),
                    range,
                    bus,
                    in settings);

            return new GroupTestContext(
                controller,
                views,
                groupView,
                input,
                targeting,
                attack,
                range);
        }

        private static IFormationProvider
            CreateFormationProvider()
        {
            return new ConfiguredFormationProvider(
                new FormationLayout(
                    SoldierGroupLevel.Level1,
                    new[]
                    {
                        Slot(0f, 0f)
                    }),
                new FormationLayout(
                    SoldierGroupLevel.Level2,
                    new[]
                    {
                        Slot(-1f, 0f),
                        Slot(1f, 0f)
                    }),
                new FormationLayout(
                    SoldierGroupLevel.Level3,
                    new[]
                    {
                        Slot(0f, 1f),
                        Slot(-1f, -1f),
                        Slot(1f, -1f)
                    }),
                new FormationLayout(
                    SoldierGroupLevel.Level4,
                    new[]
                    {
                        Slot(-1f, 1f),
                        Slot(1f, 1f),
                        Slot(-1f, -1f),
                        Slot(1f, -1f)
                    }));
        }

        private static FormationSlot Slot(
            float x,
            float z)
        {
            var point =
                new SoldierPoint(
                    x,
                    0f,
                    z);

            return new FormationSlot(
                in point);
        }

        private static SoldierSettings
            DefaultSettings()
        {
            return new SoldierSettings(
                5f,
                720f);
        }

        private sealed class GroupTestContext
        {
            public SoldierGroupController Controller { get; }
            public FakeSoldierView[] Views { get; }
            public FakeGroupView GroupView { get; }
            public SoldierGroupInputBuffer Input { get; }
            public FakeTargetingPort Targeting { get; }
            public FakeAttackPort Attack { get; }
            public FakeRangeProvider Range { get; }

            public GroupTestContext(
                SoldierGroupController controller,
                FakeSoldierView[] views,
                FakeGroupView groupView,
                SoldierGroupInputBuffer input,
                FakeTargetingPort targeting,
                FakeAttackPort attack,
                FakeRangeProvider range)
            {
                Controller = controller;
                Views = views;
                GroupView = groupView;
                Input = input;
                Targeting = targeting;
                Attack = attack;
                Range = range;
            }
        }

        private sealed class FakeSoldierView :
            ISoldierView
        {
            public SoldierPoint WorldPosition =
                SoldierPoint.Zero;

            public SoldierPoint Position =>
                WorldPosition;

            public bool Active { get; private set; }

            public SoldierPoint
                LocalFormationPosition { get; private set; }

            public float
                LastMovementSpeed { get; private set; }

            public SoldierDirection
                LastAim { get; private set; }

            public int AimCount { get; private set; }
            public int ClearAimCount { get; private set; }

            public void SetActive(bool active)
            {
                Active = active;
            }

            public void SetLocalFormationPosition(
                in SoldierPoint localPosition)
            {
                LocalFormationPosition =
                    localPosition;
            }

            public void SetMovementSpeed(
                float normalizedSpeed)
            {
                LastMovementSpeed =
                    normalizedSpeed;
            }

            public void SetAimDirection(
                in SoldierDirection direction,
                float rotationDegreesPerSecond,
                float deltaTime)
            {
                LastAim = direction;
                AimCount++;
            }

            public void ClearAim()
            {
                ClearAimCount++;
            }
        }

        private sealed class FakeGroupView :
            ISoldierGroupView
        {
            public SoldierPoint Position =>
                SoldierPoint.Zero;

            public int MoveCount { get; private set; }

            public SoldierMovementStep
                LastMovement { get; private set; }

            public void Move(
                in SoldierMovementStep movement,
                float deltaTime)
            {
                MoveCount++;
                LastMovement = movement;
            }
        }

        private sealed class FakeTargetingPort :
            ISoldierTargetingPort
        {
            public SoldierTargetInfo Result =
                SoldierTargetInfo.None;

            public int EvaluateCount;
            public int ClearCount;
            public float LastRange;

            public SoldierTargetInfo Evaluate(
                EntityId soldierId,
                in SoldierPoint position,
                float targetRange)
            {
                EvaluateCount++;
                LastRange = targetRange;
                return Result;
            }

            public void Clear(
                EntityId soldierId)
            {
                ClearCount++;
            }
        }

        private sealed class FakeAttackPort :
            ISoldierAttackPort
        {
            public int UpdateCount;
            public int ClearCount;

            public SoldierTargetInfo
                LastTarget;

            public void Update(
                EntityId soldierId,
                in SoldierTargetInfo target,
                float deltaTime)
            {
                UpdateCount++;
                LastTarget = target;
            }

            public void ClearTarget(
                EntityId soldierId)
            {
                ClearCount++;
            }
        }

        private sealed class FakeRangeProvider :
            ITargetRangeProvider
        {
            public float Range;

            public float CurrentTargetRange =>
                Range;
        }

        private sealed class FakeTargetingFactory :
            ITargetingFactory
        {
            public int CreateCount;

            public readonly Dictionary<
                long,
                FakeTargetingSession> Sessions =
                    new Dictionary<
                        long,
                        FakeTargetingSession>();

            public ITargetingSession Create(
                EntityId ownerId,
                ITargetingView view = null)
            {
                CreateCount++;

                var session =
                    new FakeTargetingSession();

                Sessions.Add(
                    ownerId.Value,
                    session);

                return session;
            }
        }

        private sealed class FakeTargetingSession :
            ITargetingSession
        {
            public int EvaluateCount;
            public int ClearCount;

            public TargetingResult Evaluate(
                in TargetingContext context)
            {
                EvaluateCount++;
                return TargetingResult.None;
            }

            public void Clear(
                TargetLossReason reason =
                    TargetLossReason.ManualClear)
            {
                ClearCount++;
            }
        }
    }
}
