using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using NUnit.Framework;
using ZombieWar.Features.Damage.Controller;
using ZombieWar.Features.Damage.Domain;
using ZombieWar.Features.Damage.Events;
using ZombieWar.Features.Damage.Model;
using ZombieWar.Features.Damage.View;
using ZombieWar.Features.Health.Factories;

namespace ZombieWar.Features.Damage.Tests
{
    public sealed class DamageFeatureTests
    {
        private sealed class FakeDamageable : IDamageable
        {
            public EntityId EntityId { get; }
            public bool IsAlive { get; set; } = true;
            public int ApplyCount { get; private set; }
            public DamageInfo LastDamage { get; private set; }

            public FakeDamageable(long id)
            {
                EntityId = new EntityId(id);
            }

            public void ApplyDamage(DamageInfo damage)
            {
                ApplyCount++;
                LastDamage = damage;
            }
        }

        private sealed class SpyDamageView : IDamageView
        {
            public int RenderCount { get; private set; }
            public DamageViewState LastState { get; private set; }

            public void Render(in DamageViewState state)
            {
                RenderCount++;
                LastState = state;
            }
        }

        [Test]
        public void Model_ValidDamage_IsAccepted()
        {
            var model = new DamageModel();
            var target = new FakeDamageable(2);
            var damage = new DamageInfo(new EntityId(1), 25f, DamageTypes.Projectile);

            var result = model.Resolve(target, in damage);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(DamageRejectionReason.None, result.RejectionReason);
            Assert.AreEqual(25f, result.RequestedAmount);
            Assert.AreEqual(25f, result.FinalAmount);
            Assert.AreEqual(new EntityId(1), result.SourceId);
            Assert.AreEqual(new EntityId(2), result.TargetId);
        }

        [Test]
        public void Model_NullTarget_IsRejected()
        {
            var model = new DamageModel();
            var damage = new DamageInfo(new EntityId(1), 25f, DamageTypes.Projectile);

            var result = model.Resolve(null, in damage);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(DamageRejectionReason.TargetMissing, result.RejectionReason);
        }

        [Test]
        public void Model_DeadTarget_IsRejected()
        {
            var model = new DamageModel();
            var target = new FakeDamageable(2) { IsAlive = false };
            var damage = new DamageInfo(new EntityId(1), 25f, DamageTypes.Projectile);

            var result = model.Resolve(target, in damage);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(DamageRejectionReason.TargetNotAlive, result.RejectionReason);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Model_NonPositiveDamage_IsRejected(float amount)
        {
            var model = new DamageModel();
            var target = new FakeDamageable(2);
            var damage = new DamageInfo(new EntityId(1), amount, DamageTypes.Projectile);

            var result = model.Resolve(target, in damage);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(DamageRejectionReason.InvalidAmount, result.RejectionReason);
        }

        [Test]
        public void Controller_AcceptedDamage_AppliesOnceAndPublishesOnce()
        {
            var bus = new EventBus();
            var view = new SpyDamageView();
            var controller = new DamageController(new DamageModel(), view, bus);
            var target = new FakeDamageable(20);
            var damage = new DamageInfo(new EntityId(10), 30f, DamageTypes.Projectile);

            int eventCount = 0;
            DamageAppliedEvent lastEvent = default;
            using (bus.Subscribe<DamageAppliedEvent>(evt =>
                   {
                       eventCount++;
                       lastEvent = evt;
                   }))
            {
                bool applied = controller.TryApply(target, damage);

                Assert.IsTrue(applied);
                Assert.AreEqual(1, target.ApplyCount);
                Assert.AreEqual(30f, target.LastDamage.Amount);
                Assert.AreEqual(DamageTypes.Projectile, target.LastDamage.Type);
                Assert.AreEqual(1, eventCount);
                Assert.AreEqual(new EntityId(10), lastEvent.SourceId);
                Assert.AreEqual(new EntityId(20), lastEvent.TargetId);
                Assert.AreEqual(30f, lastEvent.Amount);
                Assert.AreEqual(1, view.RenderCount);
                Assert.IsTrue(view.LastState.Accepted);
            }
        }

        [Test]
        public void Controller_RejectedDamage_DoesNotApplyOrPublish()
        {
            var bus = new EventBus();
            var view = new SpyDamageView();
            var controller = new DamageController(new DamageModel(), view, bus);
            var target = new FakeDamageable(20);
            var damage = new DamageInfo(new EntityId(10), 0f, DamageTypes.Projectile);

            int eventCount = 0;
            using (bus.Subscribe<DamageAppliedEvent>(_ => eventCount++))
            {
                bool applied = controller.TryApply(target, damage);

                Assert.IsFalse(applied);
                Assert.AreEqual(0, target.ApplyCount);
                Assert.AreEqual(0, eventCount);
                Assert.AreEqual(1, view.RenderCount);
                Assert.IsFalse(view.LastState.Accepted);
                Assert.AreEqual(DamageRejectionReason.InvalidAmount, view.LastState.RejectionReason);
            }
        }

        [Test]
        public void DamageAndHealth_Integration_ReducesHealthThroughIDamageable()
        {
            var bus = new EventBus();
            var healthFactory = new HealthFactory(bus);
            var health = healthFactory.Create(new EntityId(200), 100f);
            var damageController = new DamageController(
                new DamageModel(),
                NullDamageView.Instance,
                bus);

            var damage = new DamageInfo(
                new EntityId(100),
                30f,
                DamageTypes.Projectile);

            bool applied = damageController.TryApply(health, damage);

            Assert.IsTrue(applied);
            Assert.AreEqual(70f, health.CurrentHealth);
        }
    }
}
