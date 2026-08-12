using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using GameplayCore.Targeting;
using NUnit.Framework;
using ZombieWar.Features.Targeting.Controller;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Events;
using ZombieWar.Features.Targeting.Factories;
using ZombieWar.Features.Targeting.Model;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Targeting.Selection;
using ZombieWar.Features.Targeting.View;

namespace ZombieWar.Features.Targeting.Tests
{
    public sealed class TargetingFeatureTests
    {
        private sealed class FakeTarget : ITargetCandidate
        {
            public EntityId EntityId { get; set; }
            public bool IsTargetable { get; set; }
            public TargetPoint Position { get; set; }

            public FakeTarget(
                long id,
                float x,
                float z,
                bool isTargetable = true)
            {
                EntityId = new EntityId(id);
                IsTargetable = isTargetable;
                Position = new TargetPoint(x, 0f, z);
            }
        }

        private sealed class SpyView : ITargetingView
        {
            public int RenderCount { get; private set; }
            public TargetingViewState Last { get; private set; }

            public void Render(in TargetingViewState state)
            {
                RenderCount++;
                Last = state;
            }
        }

        private sealed class Rig
        {
            public readonly EventBus Bus = new EventBus();
            public readonly TargetRegistry Registry = new TargetRegistry();
            public readonly PlanarXZDistanceMetric Distance = new PlanarXZDistanceMetric();
            public readonly NearestTargetSelector Selector;
            public readonly TargetValidator Validator;

            public Rig()
            {
                Selector = new NearestTargetSelector(Distance);
                Validator = new TargetValidator(Registry, Distance);
            }

            public TargetingController CreateController(
                long ownerId,
                ITargetingView view = null)
            {
                return new TargetingController(
                    new EntityId(ownerId),
                    new TargetingModel(),
                    Registry,
                    Selector,
                    Validator,
                    view ?? NullTargetingView.Instance,
                    Bus);
            }

            public TargetingContext Context(
                float x = 0f,
                float z = 0f,
                float range = 10f)
            {
                var origin = new TargetPoint(x, 0f, z);
                return new TargetingContext(in origin, range);
            }
        }

        [Test]
        public void DistanceMetric_IgnoresY_AndUsesXZSquared()
        {
            var metric = new PlanarXZDistanceMetric();
            var a = new TargetPoint(0f, 100f, 0f);
            var b = new TargetPoint(3f, -200f, 4f);

            float sqr = metric.SqrDistance(in a, in b);

            Assert.AreEqual(25f, sqr, 0.0001f);
        }

        [Test]
        public void Registry_RegisterDuplicateAndUnregister_WorkCorrectly()
        {
            var registry = new TargetRegistry();
            var a = new FakeTarget(10, 0f, 0f);
            var b = new FakeTarget(20, 1f, 0f);

            Assert.IsTrue(registry.Register(a));
            Assert.IsFalse(registry.Register(a));
            Assert.IsTrue(registry.Register(b));
            Assert.AreEqual(2, registry.Count);
            Assert.IsTrue(registry.Contains(a.EntityId));

            Assert.IsTrue(registry.Unregister(a.EntityId));
            Assert.IsFalse(registry.Contains(a.EntityId));
            Assert.AreEqual(1, registry.Count);
            Assert.AreSame(b, registry.ActiveTargets[0]);
        }

        [Test]
        public void Selector_NoCandidate_ReturnsNull()
        {
            var rig = new Rig();
            var context = rig.Context();

            ITargetCandidate selected =
                rig.Selector.Select(
                    context,
                    rig.Registry.ActiveTargets);

            Assert.IsNull(selected);
        }

        [Test]
        public void Selector_SelectsNearestValidTargetInsideRange()
        {
            var rig = new Rig();
            var far = new FakeTarget(1, 8f, 0f);
            var near = new FakeTarget(2, 2f, 0f);
            var dead = new FakeTarget(3, 1f, 0f, false);
            var outside = new FakeTarget(4, 15f, 0f);

            rig.Registry.Register(far);
            rig.Registry.Register(near);
            rig.Registry.Register(dead);
            rig.Registry.Register(outside);

            var context = rig.Context(range: 10f);

            var selected =
                rig.Selector.Select(
                    context,
                    rig.Registry.ActiveTargets);

            Assert.AreSame(near, selected);
        }

        [Test]
        public void Selector_UsesThisSoldierOrigin_NotGroupCenter()
        {
            var rig = new Rig();
            var left = new FakeTarget(1, -5f, 0f);
            var right = new FakeTarget(2, 5f, 0f);

            rig.Registry.Register(left);
            rig.Registry.Register(right);

            var leftSoldier =
                rig.Context(x: -4f, range: 20f);

            var rightSoldier =
                rig.Context(x: 4f, range: 20f);

            Assert.AreSame(
                left,
                rig.Selector.Select(
                    leftSoldier,
                    rig.Registry.ActiveTargets));

            Assert.AreSame(
                right,
                rig.Selector.Select(
                    rightSoldier,
                    rig.Registry.ActiveTargets));
        }

        [Test]
        public void Selector_EqualDistance_UsesLowerEntityIdAsTieBreaker()
        {
            var rig = new Rig();
            var highId = new FakeTarget(20, -5f, 0f);
            var lowId = new FakeTarget(10, 5f, 0f);

            rig.Registry.Register(highId);
            rig.Registry.Register(lowId);

            var context = rig.Context(range: 10f);

            var selected =
                rig.Selector.Select(
                    context,
                    rig.Registry.ActiveTargets);

            Assert.AreSame(lowId, selected);
        }

        [Test]
        public void Validator_ValidTarget_ReturnsNone()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 3f, 4f);
            rig.Registry.Register(target);

            var handle = new TargetHandle(target);
            var context = rig.Context(range: 5f);

            Assert.AreEqual(
                TargetLossReason.None,
                rig.Validator.Validate(
                    in handle,
                    in context));
        }

        [Test]
        public void Validator_TargetNotTargetable_ReturnsReason()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 1f, 0f);
            rig.Registry.Register(target);
            var handle = new TargetHandle(target);
            target.IsTargetable = false;
            var context = rig.Context();

            Assert.AreEqual(
                TargetLossReason.NotTargetable,
                rig.Validator.Validate(
                    in handle,
                    in context));
        }

        [Test]
        public void Validator_OutOfRange_ReturnsReason()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 11f, 0f);
            rig.Registry.Register(target);
            var handle = new TargetHandle(target);
            var context = rig.Context(range: 10f);

            Assert.AreEqual(
                TargetLossReason.OutOfRange,
                rig.Validator.Validate(
                    in handle,
                    in context));
        }

        [Test]
        public void Validator_UnregisteredTarget_ReturnsReason()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 1f, 0f);
            rig.Registry.Register(target);
            var handle = new TargetHandle(target);
            rig.Registry.Unregister(target.EntityId);
            var context = rig.Context();

            Assert.AreEqual(
                TargetLossReason.Unregistered,
                rig.Validator.Validate(
                    in handle,
                    in context));
        }

        [Test]
        public void Validator_PooledIdentityChange_ReturnsIdentityChanged()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 1f, 0f);
            rig.Registry.Register(target);
            var handle = new TargetHandle(target);

            target.EntityId = new EntityId(99);
            var context = rig.Context();

            Assert.AreEqual(
                TargetLossReason.EntityIdentityChanged,
                rig.Validator.Validate(
                    in handle,
                    in context));
        }

        [Test]
        public void Controller_AcquiresNearest_AndPublishesAcquiredOnce()
        {
            var rig = new Rig();
            var far = new FakeTarget(1, 7f, 0f);
            var near = new FakeTarget(2, 2f, 0f);
            rig.Registry.Register(far);
            rig.Registry.Register(near);

            int acquiredCount = 0;
            EntityId acquiredTarget = default;

            using var subscription =
                rig.Bus.Subscribe<TargetAcquiredEvent>(
                    evt =>
                    {
                        acquiredCount++;
                        acquiredTarget = evt.TargetId;
                    });

            var controller =
                rig.CreateController(100);

            var context = rig.Context(range: 10f);
            TargetingResult result =
                controller.Evaluate(in context);

            Assert.IsTrue(result.HasTarget);
            Assert.AreEqual(near.EntityId, result.TargetId);
            Assert.AreEqual(1, acquiredCount);
            Assert.AreEqual(near.EntityId, acquiredTarget);
        }

        [Test]
        public void Controller_KeepsCurrentTarget_WhenCloserTargetAppears()
        {
            var rig = new Rig();
            var current = new FakeTarget(1, 5f, 0f);
            rig.Registry.Register(current);

            int acquiredCount = 0;
            using var subscription =
                rig.Bus.Subscribe<TargetAcquiredEvent>(
                    _ => acquiredCount++);

            var controller =
                rig.CreateController(100);

            var context = rig.Context(range: 10f);
            var first = controller.Evaluate(in context);

            var closer = new FakeTarget(2, 1f, 0f);
            rig.Registry.Register(closer);

            for (int i = 0; i < 100; i++)
            {
                var retained =
                    controller.Evaluate(in context);

                Assert.AreEqual(
                    current.EntityId,
                    retained.TargetId);
            }

            Assert.AreEqual(1, acquiredCount);
            Assert.AreEqual(
                current.EntityId,
                first.TargetId);
        }

        [Test]
        public void Controller_TargetBecomesInvalid_LosesAndReacquires()
        {
            var rig = new Rig();
            var first = new FakeTarget(1, 2f, 0f);
            var second = new FakeTarget(2, 4f, 0f);
            rig.Registry.Register(first);
            rig.Registry.Register(second);

            int lostCount = 0;
            int acquiredCount = 0;
            TargetLossReason lastReason =
                TargetLossReason.None;

            using var lostSub =
                rig.Bus.Subscribe<TargetLostEvent>(
                    evt =>
                    {
                        lostCount++;
                        lastReason = evt.Reason;
                    });

            using var acquiredSub =
                rig.Bus.Subscribe<TargetAcquiredEvent>(
                    _ => acquiredCount++);

            var controller =
                rig.CreateController(100);

            var context = rig.Context(range: 10f);
            var acquired =
                controller.Evaluate(in context);

            Assert.AreEqual(
                first.EntityId,
                acquired.TargetId);

            first.IsTargetable = false;

            var reacquired =
                controller.Evaluate(in context);

            Assert.AreEqual(1, lostCount);
            Assert.AreEqual(
                TargetLossReason.NotTargetable,
                lastReason);
            Assert.AreEqual(2, acquiredCount);
            Assert.AreEqual(
                second.EntityId,
                reacquired.TargetId);
        }

        [Test]
        public void Controller_TargetLeavesRange_ReacquiresNearestInRange()
        {
            var rig = new Rig();
            var current = new FakeTarget(1, 2f, 0f);
            var backup = new FakeTarget(2, 5f, 0f);
            rig.Registry.Register(current);
            rig.Registry.Register(backup);

            var controller =
                rig.CreateController(100);

            var initialContext =
                rig.Context(range: 10f);

            Assert.AreEqual(
                current.EntityId,
                controller.Evaluate(
                    in initialContext).TargetId);

            current.Position =
                new TargetPoint(20f, 0f, 0f);

            var next =
                controller.Evaluate(
                    in initialContext);

            Assert.AreEqual(
                backup.EntityId,
                next.TargetId);
        }

        [Test]
        public void Controller_TargetRangeShrink_CanLoseCurrentTarget()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 8f, 0f);
            rig.Registry.Register(target);

            var controller =
                rig.CreateController(100);

            var longRange =
                rig.Context(range: 10f);

            var shortRange =
                rig.Context(range: 6f);

            Assert.IsTrue(
                controller.Evaluate(
                    in longRange).HasTarget);

            Assert.IsFalse(
                controller.Evaluate(
                    in shortRange).HasTarget);
        }

        [Test]
        public void TwoSoldiers_AreAllowedToTargetSameCandidate()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 2f, 0f);
            rig.Registry.Register(target);

            var soldierA =
                rig.CreateController(100);

            var soldierB =
                rig.CreateController(200);

            var contextA =
                rig.Context(x: 0f, range: 10f);

            var contextB =
                rig.Context(x: 1f, range: 10f);

            Assert.AreEqual(
                target.EntityId,
                soldierA.Evaluate(
                    in contextA).TargetId);

            Assert.AreEqual(
                target.EntityId,
                soldierB.Evaluate(
                    in contextB).TargetId);
        }

        [Test]
        public void Controller_Clear_PublishesLostExactlyOnce()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 2f, 0f);
            rig.Registry.Register(target);

            int lostCount = 0;
            TargetLossReason reason =
                TargetLossReason.None;

            using var subscription =
                rig.Bus.Subscribe<TargetLostEvent>(
                    evt =>
                    {
                        lostCount++;
                        reason = evt.Reason;
                    });

            var controller =
                rig.CreateController(100);

            var context = rig.Context();
            controller.Evaluate(in context);

            controller.Clear();
            controller.Clear();

            Assert.AreEqual(1, lostCount);
            Assert.AreEqual(
                TargetLossReason.ManualClear,
                reason);
        }

        [Test]
        public void Factory_CreatesIndependentPerSoldierSessions()
        {
            var rig = new Rig();
            var target = new FakeTarget(1, 2f, 0f);
            rig.Registry.Register(target);

            ITargetSelector<TargetingContext, ITargetCandidate> selector =
                rig.Selector;

            var factory =
                new TargetingFactory(
                    rig.Registry,
                    selector,
                    rig.Validator,
                    rig.Bus);

            ITargetingSession a =
                factory.Create(new EntityId(100));

            ITargetingSession b =
                factory.Create(new EntityId(200));

            Assert.AreNotSame(a, b);

            var context = rig.Context();

            Assert.AreEqual(
                target.EntityId,
                a.Evaluate(in context).TargetId);

            Assert.AreEqual(
                target.EntityId,
                b.Evaluate(in context).TargetId);
        }
    }
}
