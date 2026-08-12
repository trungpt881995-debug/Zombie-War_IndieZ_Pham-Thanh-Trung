using System;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using NUnit.Framework;
using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Events;
using ZombieWar.Features.Projectile.Impact;
using ZombieWar.Features.Projectile.Model;
using ZombieWar.Features.Projectile.Motion;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Registry;
using ZombieWar.Features.Projectile.Services;

namespace ZombieWar.Features.Projectile.Tests
{
    public sealed class ProjectileFeatureTests
    {
        [Test]
        public void Model_ActivateStartsFlyingAndResetsCounters()
        {
            var model = new ProjectileModel();
            ProjectileLaunchRequest request = LinearRequest();
            model.Activate(new EntityId(10), in request);
            Assert.IsTrue(model.IsFlying);
            Assert.AreEqual(0f, model.ElapsedTime);
            Assert.AreEqual(0f, model.TravelledDistance);
            Assert.AreEqual(10, model.ProjectileId.Value);
        }

        [Test]
        public void Model_AdvanceTracksLifetimeAndDistance()
        {
            var model = new ProjectileModel();
            ProjectileLaunchRequest request = LinearRequest();
            model.Activate(new EntityId(10), in request);
            var p = new ProjectilePoint(3f, 0f, 4f);
            model.Advance(0.5f, in p);
            Assert.AreEqual(0.5f, model.ElapsedTime, 0.0001f);
            Assert.AreEqual(5f, model.TravelledDistance, 0.0001f);
        }

        [Test]
        public void Model_HitHistoryDeduplicatesEntityId()
        {
            var model = new ProjectileModel();
            ProjectileLaunchRequest request = LinearRequest(ProjectileImpactMode.Pierce);
            model.Activate(new EntityId(10), in request);
            Assert.IsTrue(model.RegisterHit(new EntityId(99)));
            Assert.IsFalse(model.RegisterHit(new EntityId(99)));
        }

        [Test]
        public void Model_ResetClearsHitHistoryAndState()
        {
            var model = new ProjectileModel();
            ProjectileLaunchRequest request = LinearRequest(ProjectileImpactMode.Pierce);
            model.Activate(new EntityId(10), in request);
            model.RegisterHit(new EntityId(99));
            model.Reset();
            Assert.AreEqual(ProjectileState.Inactive, model.State);
            Assert.IsFalse(model.HasAlreadyHit(new EntityId(99)));
        }

        [Test]
        public void LinearSolver_UsesDirectionTimesSpeed()
        {
            var solver = new LinearLaunchVelocitySolver();
            ProjectileLaunchRequest request = LinearRequest();
            Assert.IsTrue(solver.TrySolve(in request, out ProjectileVector v));
            Assert.AreEqual(20f, v.X, 0.0001f);
            Assert.AreEqual(0f, v.Y, 0.0001f);
            Assert.AreEqual(0f, v.Z, 0.0001f);
        }

        [Test]
        public void BallisticSolver_ReachesPositiveVerticalVelocityForLevelTarget()
        {
            var gravity = new ProjectileVector(0f, -9.81f, 0f);
            var solver = new BallisticLaunchVelocitySolver(in gravity);
            ProjectileLaunchRequest request = BallisticRequest();
            Assert.IsTrue(solver.TrySolve(in request, out ProjectileVector v));
            Assert.Greater(v.Y, 0f);
            Assert.Greater(v.X, 0f);
        }

        [Test]
        public void StopPolicy_DamageableMeansDamageAndComplete()
        {
            var policy = new StopOnHitPolicy();
            var model = ActivatedModel(ProjectileImpactMode.StopOnHit);
            var point = new ProjectilePoint(0f, 0f, 0f);
            var collision = ProjectileCollision.ForDamageable(new FakeDamageable(2), in point);
            ProjectileImpactDecision result = policy.Evaluate(model, in collision);
            Assert.AreEqual(ProjectileImpactAction.DamageAndComplete, result.Action);
        }

        [Test]
        public void PiercePolicy_DamageableMeansContinue()
        {
            var policy = new PiercingImpactPolicy();
            var model = ActivatedModel(ProjectileImpactMode.Pierce);
            var point = new ProjectilePoint(0f, 0f, 0f);
            var collision = ProjectileCollision.ForDamageable(new FakeDamageable(2), in point);
            ProjectileImpactDecision result = policy.Evaluate(model, in collision);
            Assert.AreEqual(ProjectileImpactAction.DamageAndContinue, result.Action);
        }

        [Test]
        public void PiercePolicy_AlreadyHitMeansIgnore()
        {
            var policy = new PiercingImpactPolicy();
            var model = ActivatedModel(ProjectileImpactMode.Pierce);
            model.RegisterHit(new EntityId(2));
            var point = new ProjectilePoint(0f, 0f, 0f);
            var collision = ProjectileCollision.ForDamageable(new FakeDamageable(2), in point);
            Assert.AreEqual(ProjectileImpactAction.Ignore, policy.Evaluate(model, in collision).Action);
        }

        [Test]
        public void GrenadePolicy_GroundMeansExplodeAndComplete()
        {
            var policy = new ExplodeOnGroundPolicy();
            var model = ActivatedModel(ProjectileImpactMode.ExplodeOnGround, 2f);
            var point = new ProjectilePoint(0f, 0f, 0f);
            var collision = ProjectileCollision.ForSurface(ProjectileCollisionKind.Ground, in point);
            Assert.AreEqual(ProjectileImpactAction.ExplodeAndComplete, policy.Evaluate(model, in collision).Action);
        }

        [Test]
        public void GrenadePolicy_DamageableDoesNotDirectHit()
        {
            var policy = new ExplodeOnGroundPolicy();
            var model = ActivatedModel(ProjectileImpactMode.ExplodeOnGround, 2f);
            var point = new ProjectilePoint(0f, 0f, 0f);
            var collision = ProjectileCollision.ForDamageable(new FakeDamageable(2), in point);
            Assert.AreEqual(ProjectileImpactAction.Ignore, policy.Evaluate(model, in collision).Action);
        }

        [Test]
        public void Controller_DirectHitAppliesDamageAndReleasesOnce()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            h.Launch();
            var point = new ProjectilePoint(1f, 0f, 0f);
            var target = new FakeDamageable(50);
            var collision = ProjectileCollision.ForDamageable(target, in point);
            h.Controller.HandleCollision(in collision);
            h.Controller.HandleCollision(in collision);
            Assert.AreEqual(1, target.ApplyCount);
            Assert.AreEqual(1, h.Pool.ReleaseCount);
            Assert.AreEqual(1, h.Damage.ApplyCount);
        }

        [Test]
        public void Controller_PierceDamagesDifferentTargetsAndStaysFlying()
        {
            var h = new Harness(ProjectileImpactMode.Pierce);
            h.Launch();
            var point = new ProjectilePoint(1f, 0f, 0f);
            var a = ProjectileCollision.ForDamageable(new FakeDamageable(1), in point);
            var b = ProjectileCollision.ForDamageable(new FakeDamageable(2), in point);
            h.Controller.HandleCollision(in a);
            h.Controller.HandleCollision(in b);
            Assert.AreEqual(2, h.Damage.ApplyCount);
            Assert.IsTrue(h.Controller.IsFlying);
            Assert.AreEqual(0, h.Pool.ReleaseCount);
        }

        [Test]
        public void Controller_PierceSameEntityTwiceDamagesOnce()
        {
            var h = new Harness(ProjectileImpactMode.Pierce);
            h.Launch();
            var point = new ProjectilePoint(1f, 0f, 0f);
            var target = new FakeDamageable(1);
            var hit = ProjectileCollision.ForDamageable(target, in point);
            h.Controller.HandleCollision(in hit);
            h.Controller.HandleCollision(in hit);
            Assert.AreEqual(1, h.Damage.ApplyCount);
        }

        [Test]
        public void Controller_GrenadeGroundRequestsExplosionExactlyOnce()
        {
            var h = new Harness(ProjectileImpactMode.ExplodeOnGround, 3f);
            h.Launch();
            var point = new ProjectilePoint(5f, 0f, 0f);
            var ground = ProjectileCollision.ForSurface(ProjectileCollisionKind.Ground, in point);
            h.Controller.HandleCollision(in ground);
            h.Controller.HandleCollision(in ground);
            Assert.AreEqual(1, h.Explosion.Count);
            Assert.AreEqual(1, h.Pool.ReleaseCount);
        }

        [Test]
        public void Controller_MaxLifetimeCompletes()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            h.Launch(maxLifetime: 0.1f);
            h.Controller.Tick(0.11f);
            Assert.AreEqual(1, h.Pool.ReleaseCount);
        }

        [Test]
        public void Controller_MaxRangeCompletes()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            h.Launch(maxRange: 2f);
            h.View.PositionValue = new ProjectilePoint(3f, 0f, 0f);
            h.Controller.Tick(0.02f);
            Assert.AreEqual(1, h.Pool.ReleaseCount);
        }

        [Test]
        public void Controller_CancelCompletesOnce()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            h.Launch();
            h.Controller.Cancel();
            h.Controller.Cancel();
            Assert.AreEqual(1, h.Pool.ReleaseCount);
        }

        [Test]
        public void ActiveRegistry_AddRemoveUsesNoDuplicateEntries()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            Assert.IsTrue(h.Registry.Add(h.Controller));
            Assert.IsFalse(h.Registry.Add(h.Controller));
            Assert.AreEqual(1, h.Registry.Count);
            Assert.IsTrue(h.Registry.Remove(h.Controller));
            Assert.AreEqual(0, h.Registry.Count);
        }

        [Test]
        public void Launcher_UsesFreshEntityIdEachLaunch()
        {
            var pool = new LauncherPool();
            var resolver = new ProjectileLaunchVelocityResolver(
                new LinearLaunchVelocitySolver(),
                new FakeBallisticSolver());
            var launcher = new ProjectileLauncher(new SequentialEntityIdGenerator(100), pool, resolver);
            ProjectileLaunchRequest request = LinearRequest();
            Assert.IsTrue(launcher.TryLaunch(in request, out EntityId a));
            pool.MakeAvailableAgain();
            Assert.IsTrue(launcher.TryLaunch(in request, out EntityId b));
            Assert.AreEqual(100, a.Value);
            Assert.AreEqual(101, b.Value);
        }

        [Test]
        public void Launcher_PoolExhaustionReturnsFalse()
        {
            var pool = new LauncherPool();
            pool.Exhausted = true;
            var resolver = new ProjectileLaunchVelocityResolver(
                new LinearLaunchVelocitySolver(),
                new FakeBallisticSolver());
            var launcher = new ProjectileLauncher(new SequentialEntityIdGenerator(), pool, resolver);
            ProjectileLaunchRequest request = LinearRequest();
            Assert.IsFalse(launcher.TryLaunch(in request, out _));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Request_InvalidSpeedThrows(float speed)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var origin = new ProjectilePoint(0,0,0);
                var target = new ProjectilePoint(10,0,0);
                var direction = new ProjectileDirection(1,0,0);
                new ProjectileLaunchRequest(new EntityId(1), new ProjectilePoolKey(1),
                    ProjectileMotionKind.Linear, ProjectileImpactMode.StopOnHit,
                    in origin, in direction, speed, 10f, 20f, 2f,
                    in target, false);
            });
        }

        [Test]
        public void Request_GrenadeRequiresPositiveExplosionRadius()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BallisticRequest(0f));
        }

        [Test]
        public void Request_BallisticRequiresTargetPoint()
        {
            var origin = new ProjectilePoint(0,0,0);
            var target = new ProjectilePoint(10,0,0);
            var direction = new ProjectileDirection(1,0,0);
            Assert.Throws<ArgumentException>(() => new ProjectileLaunchRequest(
                new EntityId(1), new ProjectilePoolKey(1), ProjectileMotionKind.Ballistic,
                ProjectileImpactMode.StopOnHit, in origin, in direction, 10f, 10f, 20f, 2f,
                in target, false));
        }

        [Test]
        public void DamageSourceIsOwnerNotProjectileId()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            h.Launch(ownerId: 777, projectileId: 999);
            var point = new ProjectilePoint(1f,0f,0f);
            var hit = ProjectileCollision.ForDamageable(new FakeDamageable(20), in point);
            h.Controller.HandleCollision(in hit);
            Assert.AreEqual(777, h.Damage.LastInfo.Source.Value);
        }

        [Test]
        public void CompletedEventEmitsExactlyOnce()
        {
            var h = new Harness(ProjectileImpactMode.StopOnHit);
            int count = 0;
            using (h.Bus.Subscribe<ProjectileCompletedEvent>(_ => count++))
            {
                h.Launch();
                h.Controller.Cancel();
                h.Controller.Cancel();
            }
            Assert.AreEqual(1, count);
        }

        private static ProjectileLaunchRequest LinearRequest(
            ProjectileImpactMode impact = ProjectileImpactMode.StopOnHit,
            float explosionRadius = 0f,
            float maxRange = 100f,
            float maxLifetime = 5f,
            long ownerId = 1)
        {
            var origin = new ProjectilePoint(0,0,0);
            var target = new ProjectilePoint(10,0,0);
            var direction = new ProjectileDirection(1,0,0);
            return new ProjectileLaunchRequest(
                new EntityId(ownerId), new ProjectilePoolKey(1), ProjectileMotionKind.Linear,
                impact, in origin, in direction, 20f, 10f, maxRange, maxLifetime,
                in target, false, explosionRadius);
        }

        private static ProjectileLaunchRequest BallisticRequest(float radius = 3f)
        {
            var origin = new ProjectilePoint(0,0,0);
            var target = new ProjectilePoint(10,0,0);
            var direction = new ProjectileDirection(1,0,0);
            return new ProjectileLaunchRequest(
                new EntityId(1), new ProjectilePoolKey(5), ProjectileMotionKind.Ballistic,
                ProjectileImpactMode.ExplodeOnGround, in origin, in direction,
                10f, 20f, 30f, 5f, in target, true, radius);
        }

        private static ProjectileModel ActivatedModel(ProjectileImpactMode mode, float radius = 0f)
        {
            ProjectileLaunchRequest request = mode == ProjectileImpactMode.ExplodeOnGround
                ? BallisticRequest(radius)
                : LinearRequest(mode);
            var model = new ProjectileModel();
            model.Activate(new EntityId(100), in request);
            return model;
        }

        private sealed class Harness
        {
            public EventBus Bus { get; } = new EventBus();
            public FakeDamageService Damage { get; } = new FakeDamageService();
            public FakeExplosionPort Explosion { get; } = new FakeExplosionPort();
            public FakeFeedbackPort Feedback { get; } = new FakeFeedbackPort();
            public FakePool Pool { get; } = new FakePool();
            public ActiveProjectileRegistry Registry { get; } = new ActiveProjectileRegistry();
            public FakeView View { get; } = new FakeView();
            public ProjectileController Controller { get; }
            private readonly ProjectileImpactMode _mode;
            private readonly float _radius;

            public Harness(ProjectileImpactMode mode, float radius = 0f)
            {
                _mode = mode; _radius = radius;
                Controller = new ProjectileController(
                    new ProjectileModel(), View, Damage, new ProjectileImpactPolicyProvider(),
                    Explosion, Feedback, Pool, Registry, Bus);
            }

            public void Launch(float maxRange = 100f, float maxLifetime = 5f, long ownerId = 1, long projectileId = 10)
            {
                ProjectileLaunchRequest request = _mode == ProjectileImpactMode.ExplodeOnGround
                    ? BallisticRequest(_radius <= 0f ? 3f : _radius)
                    : LinearRequest(_mode, 0f, maxRange, maxLifetime, ownerId);
                var velocity = new ProjectileVector(20f, 0f, 0f);
                Controller.Launch(new EntityId(projectileId), in request, in velocity);
            }
        }

        private sealed class FakeDamageable : IDamageable
        {
            public EntityId EntityId { get; }
            public bool IsAlive { get; private set; } = true;
            public int ApplyCount { get; private set; }
            public FakeDamageable(long id) => EntityId = new EntityId(id);
            public void ApplyDamage(DamageInfo damage) => ApplyCount++;
        }

        private sealed class FakeDamageService : IDamageService
        {
            public int ApplyCount { get; private set; }
            public DamageInfo LastInfo { get; private set; }
            public bool TryApply(IDamageable target, DamageInfo damage)
            {
                if (target == null || !target.IsAlive || damage.Amount <= 0f) return false;
                ApplyCount++;
                LastInfo = damage;
                target.ApplyDamage(damage);
                return true;
            }
        }

        private sealed class FakeView : IProjectileView
        {
            public ProjectilePoint PositionValue = new ProjectilePoint(0,0,0);
            public ProjectilePoint Position => PositionValue;
            public void Activate(in ProjectileViewLaunchData data) => PositionValue = data.Origin;
            public void Deactivate() { }
        }

        private sealed class FakePool : IProjectilePool
        {
            public int ReleaseCount { get; private set; }
            public ProjectileController Acquire(ProjectilePoolKey key) => null;
            public void Release(ProjectilePoolKey key, ProjectileController projectile) => ReleaseCount++;
        }

        private sealed class FakeExplosionPort : IProjectileExplosionPort
        {
            public int Count { get; private set; }
            public void Explode(in ProjectileExplosionRequest request) => Count++;
        }

        private sealed class FakeFeedbackPort : IProjectileFeedbackPort
        {
            public void OnHit(EntityId projectileId, EntityId targetId, in ProjectilePoint point) { }
            public void OnExplosion(EntityId projectileId, in ProjectilePoint point, float radius) { }
        }

        private sealed class FakeBallisticSolver : IProjectileLaunchVelocitySolver
        {
            public ProjectileMotionKind Kind => ProjectileMotionKind.Ballistic;
            public bool TrySolve(in ProjectileLaunchRequest request, out ProjectileVector velocity)
            { velocity = new ProjectileVector(1,1,0); return true; }
        }

        private sealed class LauncherPool : IProjectilePool
        {
            private ProjectileController _controller;
            public bool Exhausted;

            public LauncherPool()
            {
                var bus = new EventBus();
                var registry = new ActiveProjectileRegistry();
                _controller = new ProjectileController(
                    new ProjectileModel(), new FakeView(), new FakeDamageService(),
                    new ProjectileImpactPolicyProvider(), new FakeExplosionPort(), new FakeFeedbackPort(),
                    this, registry, bus);
            }

            public ProjectileController Acquire(ProjectilePoolKey key) => Exhausted ? null : _controller;
            public void Release(ProjectilePoolKey key, ProjectileController projectile) { }
            public void MakeAvailableAgain() => _controller.ResetForPool();
        }
    }
}
