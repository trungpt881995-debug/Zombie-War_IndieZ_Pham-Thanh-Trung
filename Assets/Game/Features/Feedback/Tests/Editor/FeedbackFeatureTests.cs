using System;
using GeneralCore.Architecture;
using NUnit.Framework;
using ZombieWar.Features.Feedback.Catalog;
using ZombieWar.Features.Feedback.Controller;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Events;
using ZombieWar.Features.Feedback.Model;
using ZombieWar.Features.Feedback.Policies;
using ZombieWar.Features.Feedback.Ports;
using ZombieWar.Features.Feedback.Services;

namespace ZombieWar.Features.Feedback.Tests
{
    public sealed class FeedbackFeatureTests
    {
        [Test]
        public void Runtime_Initial_NotInitialized()
        {
            Harness h = CreateHarness();

            Assert.That(
                h.Runtime.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initial_Mode_IsInactive()
        {
            Harness h = CreateHarness();

            Assert.That(
                h.Runtime.Mode,
                Is.EqualTo(FeedbackRuntimeMode.Inactive));
        }

        [Test]
        public void Initialize_MakesRuntimeReady()
        {
            Harness h = CreateHarness();

            h.Initialize();

            Assert.That(
                h.Runtime.IsInitialized,
                Is.True);
        }

        [Test]
        public void Shutdown_MakesRuntimeNotReady()
        {
            Harness h = ReadyHarness();

            h.Configurator.Shutdown();

            Assert.That(
                h.Runtime.IsInitialized,
                Is.False);
        }

        [Test]
        public void Play_BeforeInitialize_Rejected()
        {
            Harness h = CreateHarness();

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(result.Accepted, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(FeedbackFailure.NotInitialized));
        }

        [Test]
        public void Playing_AcceptsKnownFeedback()
        {
            Harness h = ReadyHarness();

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void Inactive_RejectsFeedback()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Inactive);

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(result.Accepted, Is.False);
        }

        [Test]
        public void Suspended_RejectsFeedback()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Suspended);

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(result.Accepted, Is.False);
        }

        [Test]
        public void TerminalDrain_AllowsTerminalSafe()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.TerminalDrain);

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.BossDefeated);

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void TerminalDrain_RejectsNormalShot()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.TerminalDrain);

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(result.Accepted, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(FeedbackFailure.TerminalRejected));
        }

        [Test]
        public void MissingDefinition_Rejected()
        {
            Harness h = ReadyHarness();
            var request = new FeedbackRequest((FeedbackId)999);

            FeedbackResult result =
                h.Runtime.Play(in request);

            Assert.That(result.Accepted, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(FeedbackFailure.DefinitionNotFound));
        }

        [Test]
        public void Accepted_IncrementsSequence()
        {
            Harness h = ReadyHarness();

            Play(h.Runtime, FeedbackId.PistolShot);
            Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                h.Runtime.Snapshot.Sequence,
                Is.EqualTo(2));
        }

        [Test]
        public void Rejected_DoesNotIncrementSequence()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Suspended);
            Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                h.Runtime.Snapshot.Sequence,
                Is.EqualTo(0));
        }

        [Test]
        public void AcceptedEvent_PublishedOnce()
        {
            Harness h = ReadyHarness();
            int count = 0;

            h.Bus.Subscribe<FeedbackPlayedEvent>(_ => count++);

            Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void RejectedEvent_PublishedOnce()
        {
            Harness h = ReadyHarness();
            int count = 0;

            h.Bus.Subscribe<FeedbackRejectedEvent>(_ => count++);
            h.Runtime.SetMode(FeedbackRuntimeMode.Suspended);

            Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void CameraChannel_Executes()
        {
            Harness h = ReadyHarness();

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Camera),
                Is.True);

            Assert.That(h.Camera.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void HapticChannel_Executes()
        {
            Harness h = ReadyHarness();

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Haptic),
                Is.True);

            Assert.That(h.Haptic.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void ScreenChannel_Executes()
        {
            Harness h = ReadyHarness();

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.SoldierDamaged);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Screen),
                Is.True);

            Assert.That(h.Screen.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void RecoilChannel_Executes()
        {
            Harness h = ReadyHarness();

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Recoil),
                Is.True);

            Assert.That(h.Recoil.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void CameraPreference_DisablesCameraOnly()
        {
            Harness h = ReadyHarness();
            h.Preferences.CameraEnabled = false;

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Camera),
                Is.False);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Haptic),
                Is.True);
        }

        [Test]
        public void HapticPreference_DisablesHapticOnly()
        {
            Harness h = ReadyHarness();
            h.Preferences.HapticEnabled = false;

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Haptic),
                Is.False);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Camera),
                Is.True);
        }

        [Test]
        public void ScreenPreference_DisablesScreen()
        {
            Harness h = ReadyHarness();
            h.Preferences.ScreenEnabled = false;

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.SoldierDamaged);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Screen),
                Is.False);
        }

        [Test]
        public void RecoilPreference_DisablesRecoil()
        {
            Harness h = ReadyHarness();
            h.Preferences.RecoilEnabled = false;

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(
                result.ExecutedChannels.HasFlag(FeedbackChannel.Recoil),
                Is.False);
        }

        [Test]
        public void AK_Haptic_IsThrottled()
        {
            Harness h = ReadyHarness();

            Play(h.Runtime, FeedbackId.AKShot);
            Play(h.Runtime, FeedbackId.AKShot);

            Assert.That(h.Haptic.PlayCount, Is.EqualTo(1));
            Assert.That(h.Camera.PlayCount, Is.EqualTo(2));
        }

        [Test]
        public void AK_Haptic_CooldownExpires()
        {
            Harness h = ReadyHarness();

            Play(h.Runtime, FeedbackId.AKShot);
            h.Runtime.Tick(0.25f);
            Play(h.Runtime, FeedbackId.AKShot);

            Assert.That(h.Haptic.PlayCount, Is.EqualTo(2));
        }

        [Test]
        public void PlayingTick_AdvancesPresentationTime()
        {
            Harness h = ReadyHarness();

            h.Runtime.Tick(1.5f);

            Assert.That(
                h.Runtime.Snapshot.Elapsed,
                Is.EqualTo(1.5f));
        }

        [Test]
        public void SuspendedTick_DoesNotAdvancePresentationTime()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Suspended);
            h.Runtime.Tick(10f);

            Assert.That(
                h.Runtime.Snapshot.Elapsed,
                Is.EqualTo(0f));
        }

        [Test]
        public void TerminalDrainTick_AdvancesPresentationTime()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.TerminalDrain);
            h.Runtime.Tick(1.5f);

            Assert.That(
                h.Runtime.Snapshot.Elapsed,
                Is.EqualTo(1.5f));
        }

        [Test]
        public void InactiveTick_DoesNotAdvancePresentationTime()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Inactive);
            h.Runtime.Tick(1.5f);

            Assert.That(
                h.Runtime.Snapshot.Elapsed,
                Is.EqualTo(0f));
        }

        [Test]
        public void Suspended_SuspendsScreen()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Suspended);

            Assert.That(h.Screen.Suspended, Is.True);
        }

        [Test]
        public void Playing_UnsuspendsScreen()
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(FeedbackRuntimeMode.Suspended);
            h.Runtime.SetMode(FeedbackRuntimeMode.Playing);

            Assert.That(h.Screen.Suspended, Is.False);
        }

        [Test]
        public void Inactive_ClearsOutputs()
        {
            Harness h = ReadyHarness();

            int before = h.Screen.ClearCount;

            h.Runtime.SetMode(FeedbackRuntimeMode.Inactive);

            Assert.That(
                h.Screen.ClearCount,
                Is.GreaterThan(before));
        }

        [Test]
        public void CancelAll_CancelsEveryOutput()
        {
            Harness h = ReadyHarness();

            int cameraBefore = h.Camera.CancelCount;
            int hapticBefore = h.Haptic.CancelCount;
            int recoilBefore = h.Recoil.CancelCount;
            int screenBefore = h.Screen.ClearCount;

            h.Runtime.CancelAll();

            Assert.That(
                h.Camera.CancelCount,
                Is.EqualTo(cameraBefore + 1));

            Assert.That(
                h.Haptic.CancelCount,
                Is.EqualTo(hapticBefore + 1));

            Assert.That(
                h.Recoil.CancelCount,
                Is.EqualTo(recoilBefore + 1));

            Assert.That(
                h.Screen.ClearCount,
                Is.EqualTo(screenBefore + 1));
        }

        [Test]
        public void CriticalCamera_CanReplaceLowPriority()
        {
            Harness h = ReadyHarness();

            Play(h.Runtime, FeedbackId.PistolShot);
            Play(h.Runtime, FeedbackId.BossDefeated);

            Assert.That(h.Camera.PlayCount, Is.EqualTo(2));
        }

        [Test]
        public void LowCamera_CannotOverrideActiveCritical()
        {
            Harness h = ReadyHarness();

            Play(h.Runtime, FeedbackId.BossDefeated);
            Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(h.Camera.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void PriorityOccupation_Expires()
        {
            Harness h = ReadyHarness();

            Play(h.Runtime, FeedbackId.BossDefeated);
            h.Runtime.Tick(1f);
            Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(h.Camera.PlayCount, Is.EqualTo(2));
        }

        [Test]
        public void RequestIntensity_ScalesScreen()
        {
            Harness h = ReadyHarness();
            var request = new FeedbackRequest(
                FeedbackId.SoldierDamaged,
                0.5f);

            h.Runtime.Play(in request);

            Assert.That(
                h.Screen.LastIntensity,
                Is.EqualTo(0.15f).Within(0.0001f));
        }

        [Test]
        public void RequestIntensity_ScalesRecoil()
        {
            Harness h = ReadyHarness();
            var request = new FeedbackRequest(
                FeedbackId.PistolShot,
                0.5f);

            h.Runtime.Play(in request);

            Assert.That(
                h.Recoil.LastStrength,
                Is.EqualTo(0.1f).Within(0.0001f));
        }

        [Test]
        public void AcceptedRequest_CanExecuteNoChannels()
        {
            Harness h = ReadyHarness();

            h.Preferences.CameraEnabled = false;
            h.Preferences.HapticEnabled = false;
            h.Preferences.ScreenEnabled = false;
            h.Preferences.RecoilEnabled = false;

            FeedbackResult result =
                Play(h.Runtime, FeedbackId.PistolShot);

            Assert.That(result.Accepted, Is.True);
            Assert.That(
                result.ExecutedChannels,
                Is.EqualTo(FeedbackChannel.None));
        }

        [Test]
        public void Catalog_RejectsDuplicateIds()
        {
            FeedbackRecipe recipe =
                Recipe(FeedbackId.PistolShot);

            Assert.Throws<InvalidOperationException>(
                () => new FeedbackCatalog(
                    new[]
                    {
                        recipe,
                        recipe
                    }));
        }

        [Test]
        public void Catalog_MissingId_ReturnsFalse()
        {
            var catalog = new FeedbackCatalog(
                new[]
                {
                    Recipe(FeedbackId.PistolShot)
                });

            Assert.That(
                catalog.TryGet(
                    FeedbackId.AKShot,
                    out _),
                Is.False);
        }

        [Test]
        public void Request_None_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FeedbackRequest(FeedbackId.None));
        }

        [Test]
        public void Request_ZeroIntensity_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FeedbackRequest(
                    FeedbackId.PistolShot,
                    0f));
        }

        [Test]
        public void InvalidMode_Throws()
        {
            Harness h = ReadyHarness();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => h.Runtime.SetMode((FeedbackRuntimeMode)99));
        }

        [Test]
        public void NegativeTick_Throws()
        {
            Harness h = ReadyHarness();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => h.Runtime.Tick(-1f));
        }

        [TestCase(FeedbackId.PistolShot)]
        [TestCase(FeedbackId.AKShot)]
        [TestCase(FeedbackId.ShotgunShot)]
        [TestCase(FeedbackId.SniperShot)]
        [TestCase(FeedbackId.GrenadeShot)]
        [TestCase(FeedbackId.FlamethrowerStart)]
        [TestCase(FeedbackId.SoldierDamaged)]
        [TestCase(FeedbackId.SoldierCriticalDamage)]
        [TestCase(FeedbackId.GrenadeExplosion)]
        [TestCase(FeedbackId.BossHit)]
        [TestCase(FeedbackId.BossDefeated)]
        [TestCase(FeedbackId.SoldierGroupLevelUp)]
        [TestCase(FeedbackId.GameOver)]
        [TestCase(FeedbackId.LevelComplete)]
        [TestCase(FeedbackId.EndGame)]
        public void Catalog_ContainsExpectedIds(FeedbackId id)
        {
            Harness h = ReadyHarness();

            Assert.That(
                h.Catalog.TryGet(
                    id,
                    out _),
                Is.True);
        }

        [TestCase(FeedbackRuntimeMode.Inactive)]
        [TestCase(FeedbackRuntimeMode.Playing)]
        [TestCase(FeedbackRuntimeMode.Suspended)]
        [TestCase(FeedbackRuntimeMode.TerminalDrain)]
        public void Mode_CanBeSet(FeedbackRuntimeMode mode)
        {
            Harness h = ReadyHarness();

            h.Runtime.SetMode(mode);

            Assert.That(
                h.Runtime.Mode,
                Is.EqualTo(mode));
        }

        private static Harness ReadyHarness()
        {
            Harness h = CreateHarness();

            h.Initialize();
            h.Runtime.SetMode(FeedbackRuntimeMode.Playing);

            return h;
        }

        private static Harness CreateHarness()
        {
            var bus = new EventBus();
            var model = new FeedbackModel();
            var preferences = new FeedbackPreferences();
            var hapticCooldown = new HapticCooldownPolicy();
            var priority = new FeedbackPriorityPolicy();

            var controller = new FeedbackController(
                model,
                bus,
                hapticCooldown,
                priority,
                preferences);

            var runtime = new FeedbackRuntime(controller);

            return new Harness(
                bus,
                runtime,
                runtime,
                preferences,
                CreateCatalog(),
                new FakeCamera(),
                new FakeHaptic(),
                new FakeScreen(),
                new FakeRecoil());
        }

        private static FeedbackResult Play(
            IFeedbackRuntime runtime,
            FeedbackId id)
        {
            var request = new FeedbackRequest(id);

            return runtime.Play(in request);
        }

        private static IFeedbackCatalog CreateCatalog()
        {
            return new FeedbackCatalog(
                new[]
                {
                    Recipe(
                        FeedbackId.PistolShot,
                        camera: true,
                        haptic: true,
                        recoil: true),

                    Recipe(
                        FeedbackId.AKShot,
                        camera: true,
                        haptic: true,
                        recoil: true,
                        hapticCooldown: 0.2f),

                    Recipe(
                        FeedbackId.ShotgunShot,
                        camera: true,
                        haptic: true,
                        recoil: true,
                        priority: FeedbackPriority.High),

                    Recipe(
                        FeedbackId.SniperShot,
                        camera: true,
                        haptic: true,
                        recoil: true,
                        priority: FeedbackPriority.High),

                    Recipe(
                        FeedbackId.GrenadeShot,
                        camera: true,
                        haptic: true,
                        recoil: true),

                    Recipe(
                        FeedbackId.FlamethrowerStart,
                        haptic: true),

                    Recipe(
                        FeedbackId.SoldierDamaged,
                        camera: true,
                        haptic: true,
                        screen: true),

                    Recipe(
                        FeedbackId.SoldierCriticalDamage,
                        camera: true,
                        haptic: true,
                        screen: true,
                        priority: FeedbackPriority.High),

                    Recipe(
                        FeedbackId.GrenadeExplosion,
                        camera: true,
                        haptic: true,
                        screen: true,
                        terminal: true,
                        priority: FeedbackPriority.High),

                    Recipe(
                        FeedbackId.BossHit,
                        camera: true),

                    Recipe(
                        FeedbackId.BossDefeated,
                        camera: true,
                        haptic: true,
                        screen: true,
                        terminal: true,
                        priority: FeedbackPriority.Critical,
                        cameraDuration: 0.5f),

                    Recipe(
                        FeedbackId.SoldierGroupLevelUp,
                        camera: true,
                        haptic: true,
                        screen: true,
                        priority: FeedbackPriority.High),

                    Recipe(
                        FeedbackId.GameOver,
                        camera: true,
                        haptic: true,
                        screen: true,
                        terminal: true,
                        priority: FeedbackPriority.Critical),

                    Recipe(
                        FeedbackId.LevelComplete,
                        camera: true,
                        haptic: true,
                        screen: true,
                        terminal: true,
                        priority: FeedbackPriority.Critical),

                    Recipe(
                        FeedbackId.EndGame,
                        camera: true,
                        haptic: true,
                        screen: true,
                        terminal: true,
                        priority: FeedbackPriority.Critical)
                });
        }

        private static FeedbackRecipe Recipe(
            FeedbackId id,
            bool camera = false,
            bool haptic = false,
            bool screen = false,
            bool recoil = false,
            bool terminal = false,
            FeedbackPriority priority = FeedbackPriority.Normal,
            float hapticCooldown = 0f,
            float cameraDuration = 0.1f)
        {
            var cameraDefinition =
                new CameraFeedbackDefinition(
                    camera,
                    FeedbackCameraCue.LightWeapon,
                    cameraDuration);

            var hapticDefinition =
                new HapticFeedbackDefinition(
                    haptic,
                    HapticFeedbackStrength.Light,
                    hapticCooldown);

            var screenDefinition =
                new ScreenFeedbackDefinition(
                    screen,
                    ScreenFeedbackKind.Damage,
                    screen ? 0.3f : 0f,
                    screen ? 0.2f : 0f);

            var recoilDefinition =
                new RecoilFeedbackDefinition(
                    recoil,
                    recoil ? 0.2f : 0f,
                    recoil ? 0.1f : 0f);

            return new FeedbackRecipe(
                id,
                priority,
                terminal,
                in cameraDefinition,
                in hapticDefinition,
                in screenDefinition,
                in recoilDefinition);
        }

        private sealed class Harness
        {
            public Harness(
                EventBus bus,
                IFeedbackRuntime runtime,
                IFeedbackRuntimeConfigurator configurator,
                FeedbackPreferences preferences,
                IFeedbackCatalog catalog,
                FakeCamera camera,
                FakeHaptic haptic,
                FakeScreen screen,
                FakeRecoil recoil)
            {
                Bus = bus;
                Runtime = runtime;
                Configurator = configurator;
                Preferences = preferences;
                Catalog = catalog;
                Camera = camera;
                Haptic = haptic;
                Screen = screen;
                Recoil = recoil;
            }

            public EventBus Bus { get; }
            public IFeedbackRuntime Runtime { get; }
            public IFeedbackRuntimeConfigurator Configurator { get; }
            public FeedbackPreferences Preferences { get; }
            public IFeedbackCatalog Catalog { get; }
            public FakeCamera Camera { get; }
            public FakeHaptic Haptic { get; }
            public FakeScreen Screen { get; }
            public FakeRecoil Recoil { get; }

            public void Initialize()
            {
                Configurator.Initialize(
                    Catalog,
                    Camera,
                    Haptic,
                    Screen,
                    Recoil);
            }
        }

        private sealed class FakeCamera : ICameraFeedbackPort
        {
            public int PlayCount { get; private set; }
            public int CancelCount { get; private set; }

            public bool TryPlay(FeedbackCameraCue cue)
            {
                PlayCount++;
                return true;
            }

            public void CancelAll()
            {
                CancelCount++;
            }
        }

        private sealed class FakeHaptic : IHapticFeedbackPort
        {
            public int PlayCount { get; private set; }
            public int CancelCount { get; private set; }

            public bool TryPlay(HapticFeedbackStrength strength)
            {
                PlayCount++;
                return true;
            }

            public void CancelAll()
            {
                CancelCount++;
            }
        }

        private sealed class FakeScreen : IScreenFeedbackPort
        {
            public int PlayCount { get; private set; }
            public int ClearCount { get; private set; }
            public bool Suspended { get; private set; }
            public float LastIntensity { get; private set; }

            public bool TryFlash(
                ScreenFeedbackKind kind,
                float intensity,
                float duration)
            {
                PlayCount++;
                LastIntensity = intensity;

                return true;
            }

            public void SetSuspended(bool suspended)
            {
                Suspended = suspended;
            }

            public void Clear()
            {
                ClearCount++;
            }
        }

        private sealed class FakeRecoil : IRecoilFeedbackPort
        {
            public int PlayCount { get; private set; }
            public int CancelCount { get; private set; }
            public float LastStrength { get; private set; }

            public bool TryApply(
                float strength,
                float duration)
            {
                PlayCount++;
                LastStrength = strength;

                return true;
            }

            public void CancelAll()
            {
                CancelCount++;
            }
        }
    }
}
