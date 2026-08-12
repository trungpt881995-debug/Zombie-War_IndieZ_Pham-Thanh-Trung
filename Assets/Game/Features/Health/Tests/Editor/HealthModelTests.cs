using System;
using NUnit.Framework;
using ZombieWar.Features.Health.Model;

namespace ZombieWar.Features.Health.Tests
{
    public sealed class HealthModelTests
    {
        [Test]
        public void Constructor_WithValidMaxHealth_StartsFull()
        {
            var model = new HealthModel(100f);
            Assert.AreEqual(100f, model.CurrentHealth);
            Assert.AreEqual(100f, model.MaxHealth);
            Assert.AreEqual(1f, model.NormalizedHealth);
            Assert.IsTrue(model.IsAlive);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_WithNonPositiveMaxHealth_Throws(float maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthModel(maxHealth));
        }

        [Test]
        public void Damage_ReducesCurrentHealth()
        {
            var model = new HealthModel(100f);
            var result = model.Reduce(30f);
            Assert.AreEqual(70f, model.CurrentHealth);
            Assert.AreEqual(30f, result.AppliedAmount);
            Assert.IsTrue(result.Changed);
            Assert.IsFalse(result.BecameDepleted);
        }

        [Test]
        public void Damage_GreaterThanHealth_ClampsToZero()
        {
            var model = new HealthModel(100f);
            var result = model.Reduce(150f);
            Assert.AreEqual(0f, model.CurrentHealth);
            Assert.AreEqual(100f, result.AppliedAmount);
            Assert.IsTrue(result.BecameDepleted);
        }

        [Test]
        public void Damage_WhenAlreadyDepleted_DoesNothing()
        {
            var model = new HealthModel(100f);
            model.Reduce(100f);
            var result = model.Reduce(10f);
            Assert.AreEqual(0f, model.CurrentHealth);
            Assert.IsFalse(result.Changed);
            Assert.IsFalse(result.BecameDepleted);
        }

        [Test]
        public void ZeroOrNegativeDamage_DoesNothing()
        {
            var model = new HealthModel(100f);
            Assert.IsFalse(model.Reduce(0f).Changed);
            Assert.IsFalse(model.Reduce(-10f).Changed);
            Assert.AreEqual(100f, model.CurrentHealth);
        }

        [Test]
        public void Reset_RestoresMaxHealth()
        {
            var model = new HealthModel(100f);
            model.Reduce(40f);
            var result = model.Reset();
            Assert.AreEqual(100f, model.CurrentHealth);
            Assert.IsTrue(result.Changed);
        }

        [Test]
        public void NormalizedHealth_IsCorrect()
        {
            var model = new HealthModel(200f);
            model.Reduce(50f);
            Assert.AreEqual(0.75f, model.NormalizedHealth, 0.0001f);
        }
    }
}
