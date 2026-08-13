using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using NUnit.Framework;
using ZombieWar.Features.Audio.Catalog;
using ZombieWar.Features.Audio.Controller;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Model;
using ZombieWar.Features.Audio.Policies;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Services;

namespace ZombieWar.Features.Audio.Tests
{
    public sealed class AudioFeatureTests
    {
        [Test]
        public void Catalog_BuildsDefinitions()
        {
            var catalog =
                new AudioCatalog(
                    new[]
                    {
                        Definition(AudioId.PistolFire),
                        Definition(AudioId.AKFire)
                    });

            Assert.That(catalog.Count, Is.EqualTo(2));
            Assert.That(
                catalog.TryGet(
                    AudioId.PistolFire,
                    out AudioDefinition definition),
                Is.True);

            Assert.That(
                definition.Id,
                Is.EqualTo(AudioId.PistolFire));
        }

        [Test]
        public void Catalog_DuplicateId_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    _ = new AudioCatalog(
                        new[]
                        {
                            Definition(AudioId.PistolFire),
                            Definition(AudioId.PistolFire)
                        });
                });
        }

        [Test]
        public void Definition_None_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = Definition(AudioId.None);
                });
        }

        [Test]
        public void Definition_ZeroConcurrency_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new AudioDefinition(
                        AudioId.PistolFire,
                        AudioCategory.SFX,
                        AudioLifetimeMode.OneShot,
                        AudioSpatialMode.TwoD,
                        AudioPriority.Normal,
                        0,
                        1f,
                        1f,
                        1f,
                        0f,
                        0f,
                        false);
                });
        }

        [Test]
        public void Definition_InvalidPitch_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    _ = new AudioDefinition(
                        AudioId.PistolFire,
                        AudioCategory.SFX,
                        AudioLifetimeMode.OneShot,
                        AudioSpatialMode.TwoD,
                        AudioPriority.Normal,
                        1,
                        1f,
                        0f,
                        1f,
                        0f,
                        0f,
                        false);
                });
        }

        [Test]
        public void Runtime_PlayBeforeInitialize_Rejected()
        {
            TestRig rig = CreateRig(
                Definition(AudioId.PistolFire));

            var request =
                new AudioRequest(AudioId.PistolFire);

            AudioPlayResult result =
                rig.Runtime.Play(in request);

            Assert.That(result.Accepted, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(AudioFailure.NotInitialized));
        }

        [Test]
        public void Runtime_Initialize_AppliesDesiredMode()
        {
            TestRig rig = CreateRig(
                Definition(AudioId.PistolFire));

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Playing);

            rig.Initialize();

            Assert.That(
                rig.Runtime.WorldMode,
                Is.EqualTo(WorldAudioMode.Playing));
        }

        [Test]
        public void Runtime_ValidOneShot_Accepted()
        {
            TestRig rig = CreateInitializedRig(
                Definition(AudioId.PistolFire));

            AudioPlayResult result =
                Play(rig, AudioId.PistolFire);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Handle.IsValid, Is.False);
            Assert.That(rig.Model.ActiveVoiceCount, Is.EqualTo(1));
            Assert.That(rig.Model.PlayedCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_Looping_ReturnsHandle()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            AudioPlayResult result =
                Play(
                    rig,
                    AudioId.FlamethrowerLoop);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.Handle.IsValid, Is.True);
            Assert.That(rig.Model.ActiveVoiceCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_IsPlaying_ReturnsTrueForActiveLoop()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            AudioPlayResult result =
                Play(
                    rig,
                    AudioId.FlamethrowerLoop);

            Assert.That(
                rig.Runtime.IsPlaying(result.Handle),
                Is.True);
        }

        [Test]
        public void Runtime_IsPlaying_ReturnsFalseAfterModeReleasesLoop()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            AudioPlayResult result =
                Play(
                    rig,
                    AudioId.FlamethrowerLoop);

            rig.Runtime.SetWorldMode(
                WorldAudioMode.TerminalDrain);

            Assert.That(
                rig.Runtime.IsPlaying(result.Handle),
                Is.False);
        }

        [Test]
        public void Runtime_StopLoop_ReleasesVoice()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            AudioPlayResult result =
                Play(
                    rig,
                    AudioId.FlamethrowerLoop);

            bool stopped =
                rig.Runtime.Stop(result.Handle);

            Assert.That(stopped, Is.True);
            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
            Assert.That(rig.Model.ReleasedCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_StopInvalidHandle_IsSafe()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            Assert.That(
                rig.Runtime.Stop(AudioHandle.Invalid),
                Is.False);
        }

        [Test]
        public void Runtime_StopUnknownHandle_IsSafe()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            Assert.That(
                rig.Runtime.Stop(new AudioHandle(999)),
                Is.False);
        }

        [Test]
        public void Runtime_OneShotCompletion_AutoReleases()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            Play(rig, AudioId.PistolFire);

            FakeAudioVoice voice =
                rig.Pool.LastAcquired;

            voice.Complete();

            rig.Runtime.Tick(0.1f);

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
            Assert.That(voice.ReleaseCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_CancelAll_ReleasesAll()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        maxConcurrent: 4),
                    poolCapacity: 4);

            Play(rig, AudioId.PistolFire);
            Play(rig, AudioId.PistolFire);
            Play(rig, AudioId.PistolFire);

            rig.Runtime.CancelAll();

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
            Assert.That(rig.Model.ReleasedCount, Is.EqualTo(3));
        }

        [Test]
        public void Runtime_CancelAll_Twice_IsIdempotent()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            Play(rig, AudioId.PistolFire);

            rig.Runtime.CancelAll();
            rig.Runtime.CancelAll();

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
            Assert.That(rig.Pool.TotalReleaseCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_Inactive_RejectsWorldSfx()
        {
            TestRig rig = CreateRig(
                Definition(AudioId.PistolFire));

            rig.Initialize(
                WorldAudioMode.Inactive);

            AudioPlayResult result =
                Play(rig, AudioId.PistolFire);

            Assert.That(result.Accepted, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(AudioFailure.WorldModeRejected));
        }

        [Test]
        public void Runtime_Inactive_AllowsUi()
        {
            TestRig rig =
                CreateRig(
                    Definition(
                        AudioId.UIButtonClick,
                        category: AudioCategory.UI,
                        spatial: AudioSpatialMode.TwoD));

            rig.Initialize(
                WorldAudioMode.Inactive);

            AudioPlayResult result =
                Play(rig, AudioId.UIButtonClick);

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void Runtime_Suspended_RejectsNewWorldSfx()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Suspended);

            AudioPlayResult result =
                Play(rig, AudioId.PistolFire);

            Assert.That(result.Accepted, Is.False);
        }

        [Test]
        public void Runtime_Suspended_PausesExistingWorldVoice()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            Play(
                rig,
                AudioId.FlamethrowerLoop);

            FakeAudioVoice voice =
                rig.Pool.LastAcquired;

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Suspended);

            Assert.That(voice.IsPaused, Is.True);
        }

        [Test]
        public void Runtime_Resume_UnpausesExistingWorldVoice()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            Play(
                rig,
                AudioId.FlamethrowerLoop);

            FakeAudioVoice voice =
                rig.Pool.LastAcquired;

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Suspended);

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Playing);

            Assert.That(voice.IsPaused, Is.False);
        }

        [Test]
        public void Runtime_TerminalDrain_AllowsTerminalSafeOneShot()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.BossDeath,
                        terminalSafe: true,
                        priority: AudioPriority.Critical));

            rig.Runtime.SetWorldMode(
                WorldAudioMode.TerminalDrain);

            AudioPlayResult result =
                Play(rig, AudioId.BossDeath);

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void Runtime_TerminalDrain_RejectsUnsafeOneShot()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            rig.Runtime.SetWorldMode(
                WorldAudioMode.TerminalDrain);

            AudioPlayResult result =
                Play(rig, AudioId.PistolFire);

            Assert.That(result.Accepted, Is.False);
        }

        [Test]
        public void Runtime_TerminalDrain_StopsExistingLoop()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping));

            Play(
                rig,
                AudioId.FlamethrowerLoop);

            rig.Runtime.SetWorldMode(
                WorldAudioMode.TerminalDrain);

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
        }

        [Test]
        public void Runtime_TerminalDrain_UnpausesExistingOneShot()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.GrenadeExplosion,
                        terminalSafe: true));

            Play(
                rig,
                AudioId.GrenadeExplosion);

            FakeAudioVoice voice =
                rig.Pool.LastAcquired;

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Suspended);

            Assert.That(voice.IsPaused, Is.True);

            rig.Runtime.SetWorldMode(
                WorldAudioMode.TerminalDrain);

            Assert.That(voice.IsPaused, Is.False);
        }

        [Test]
        public void Runtime_Inactive_ReleasesExistingWorldVoices()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            Play(rig, AudioId.PistolFire);

            rig.Runtime.SetWorldMode(
                WorldAudioMode.Inactive);

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
        }

        [Test]
        public void Runtime_ConcurrencyLimit_RejectsExcess()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.ZombieDeath,
                        maxConcurrent: 2),
                    poolCapacity: 4);

            Assert.That(
                Play(rig, AudioId.ZombieDeath).Accepted,
                Is.True);

            Assert.That(
                Play(rig, AudioId.ZombieDeath).Accepted,
                Is.True);

            AudioPlayResult third =
                Play(rig, AudioId.ZombieDeath);

            Assert.That(third.Accepted, Is.False);
            Assert.That(
                third.Failure,
                Is.EqualTo(AudioFailure.ConcurrencyLimited));
        }

        [Test]
        public void Runtime_PoolExhaustion_RejectsWhenNoStealCandidate()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.BossDeath,
                        priority: AudioPriority.Critical,
                        maxConcurrent: 2),
                    poolCapacity: 1);

            Play(rig, AudioId.BossDeath);

            AudioPlayResult second =
                Play(rig, AudioId.BossDeath);

            Assert.That(second.Accepted, Is.False);
            Assert.That(
                second.Failure,
                Is.EqualTo(AudioFailure.PoolExhausted));
        }

        [Test]
        public void Runtime_CriticalVoice_StealsLowerPriorityOneShot()
        {
            TestRig rig =
                CreateInitializedRig(
                    new[]
                    {
                        Definition(
                            AudioId.ZombieHit,
                            priority: AudioPriority.Low),
                        Definition(
                            AudioId.BossDeath,
                            priority: AudioPriority.Critical,
                            terminalSafe: true)
                    },
                    poolCapacity: 1);

            Assert.That(
                Play(rig, AudioId.ZombieHit).Accepted,
                Is.True);

            AudioPlayResult critical =
                Play(rig, AudioId.BossDeath);

            Assert.That(critical.Accepted, Is.True);
            Assert.That(rig.Model.ActiveVoiceCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_DoesNotStealLoopingVoice()
        {
            TestRig rig =
                CreateInitializedRig(
                    new[]
                    {
                        Definition(
                            AudioId.FlamethrowerLoop,
                            lifetime: AudioLifetimeMode.Looping,
                            priority: AudioPriority.Low),
                        Definition(
                            AudioId.BossDeath,
                            priority: AudioPriority.Critical)
                    },
                    poolCapacity: 1);

            Play(
                rig,
                AudioId.FlamethrowerLoop);

            AudioPlayResult critical =
                Play(rig, AudioId.BossDeath);

            Assert.That(critical.Accepted, Is.False);
        }

        [Test]
        public void Runtime_ThreeDWithoutPositionOrAnchor_Rejected()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        spatial: AudioSpatialMode.ThreeD));

            var request =
                new AudioRequest(AudioId.PistolFire);

            AudioPlayResult result =
                rig.Runtime.Play(in request);

            Assert.That(result.Accepted, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(AudioFailure.InvalidSpatialContext));
        }

        [Test]
        public void Runtime_ThreeDWithPosition_Accepted()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        spatial: AudioSpatialMode.ThreeD));

            var point =
                new AudioPoint(1f, 2f, 3f);

            var request =
                new AudioRequest(
                    AudioId.PistolFire,
                    in point);

            AudioPlayResult result =
                rig.Runtime.Play(in request);

            Assert.That(result.Accepted, Is.True);
            Assert.That(
                rig.Pool.LastAcquired.LastPosition.X,
                Is.EqualTo(1f));
        }

        [Test]
        public void Runtime_ValidAnchor_Accepted()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping,
                        spatial: AudioSpatialMode.ThreeD));

            var anchor =
                new FakeAudioAnchor(
                    new AudioPoint(2f, 0f, 4f));

            var request =
                new AudioRequest(
                    AudioId.FlamethrowerLoop,
                    anchor);

            AudioPlayResult result =
                rig.Runtime.Play(in request);

            Assert.That(result.Accepted, Is.True);
        }

        [Test]
        public void Runtime_InvalidAnchor_Rejected()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping,
                        spatial: AudioSpatialMode.ThreeD));

            var anchor =
                new FakeAudioAnchor(
                    new AudioPoint(0f, 0f, 0f),
                    false);

            var request =
                new AudioRequest(
                    AudioId.FlamethrowerLoop,
                    anchor);

            AudioPlayResult result =
                rig.Runtime.Play(in request);

            Assert.That(result.Accepted, Is.False);
        }

        [Test]
        public void Runtime_AnchorFollow_UpdatesVoicePosition()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping,
                        spatial: AudioSpatialMode.ThreeD));

            var anchor =
                new FakeAudioAnchor(
                    new AudioPoint(1f, 0f, 1f));

            var request =
                new AudioRequest(
                    AudioId.FlamethrowerLoop,
                    anchor);

            rig.Runtime.Play(in request);

            anchor.Position =
                new AudioPoint(7f, 0f, 9f);

            rig.Runtime.Tick(0.1f);

            Assert.That(
                rig.Pool.LastAcquired.LastPosition.X,
                Is.EqualTo(7f));

            Assert.That(
                rig.Pool.LastAcquired.LastPosition.Z,
                Is.EqualTo(9f));
        }

        [Test]
        public void Runtime_AnchorLost_ReleasesVoice()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.FlamethrowerLoop,
                        lifetime: AudioLifetimeMode.Looping,
                        spatial: AudioSpatialMode.ThreeD));

            var anchor =
                new FakeAudioAnchor(
                    new AudioPoint(1f, 0f, 1f));

            var request =
                new AudioRequest(
                    AudioId.FlamethrowerLoop,
                    anchor);

            rig.Runtime.Play(in request);

            anchor.IsValid = false;
            rig.Runtime.Tick(0.1f);

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
        }

        [Test]
        public void Runtime_PreferencesScaleSfxVolume()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        spatial: AudioSpatialMode.TwoD));

            rig.Preferences.MasterVolume = 0.5f;
            rig.Preferences.SFXVolume = 0.5f;

            Play(rig, AudioId.PistolFire);

            Assert.That(
                rig.Pool.LastAcquired.Volume,
                Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void Runtime_MuteProducesZeroVolumeWithoutRejecting()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        spatial: AudioSpatialMode.TwoD));

            rig.Preferences.Muted = true;

            AudioPlayResult result =
                Play(rig, AudioId.PistolFire);

            Assert.That(result.Accepted, Is.True);
            Assert.That(
                rig.Pool.LastAcquired.Volume,
                Is.Zero);
        }

        [Test]
        public void Runtime_Intensity_IsClamped()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        spatial: AudioSpatialMode.TwoD));

            var request =
                new AudioRequest(
                    AudioId.PistolFire,
                    intensity: 10f);

            rig.Runtime.Play(in request);

            Assert.That(
                rig.Pool.LastAcquired.Volume,
                Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void Runtime_RandomPitch_IsApplied()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        minPitch: 0.9f,
                        maxPitch: 1.1f,
                        spatial: AudioSpatialMode.TwoD));

            rig.Random.Value = 1.05f;

            Play(rig, AudioId.PistolFire);

            Assert.That(
                rig.Pool.LastAcquired.Pitch,
                Is.EqualTo(1.05f));
        }

        [Test]
        public void Runtime_PlayedSequence_IncrementsAcceptedOnly()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.PistolFire,
                        spatial: AudioSpatialMode.TwoD));

            Play(rig, AudioId.PistolFire);

            var bad =
                new AudioRequest(AudioId.None);

            rig.Runtime.Play(in bad);

            Play(rig, AudioId.PistolFire);

            Assert.That(rig.Model.PlayedCount, Is.EqualTo(2));
            Assert.That(rig.Model.RejectedCount, Is.EqualTo(1));
            Assert.That(rig.Model.Sequence, Is.EqualTo(2));
        }

        [Test]
        public void Runtime_Shutdown_ReleasesVoicesAndResetsMode()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(AudioId.PistolFire));

            Play(rig, AudioId.PistolFire);

            rig.Configurator.Shutdown();

            Assert.That(rig.Runtime.IsInitialized, Is.False);
            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
            Assert.That(
                rig.Model.WorldMode,
                Is.EqualTo(WorldAudioMode.Inactive));
        }

        [Test]
        public void Runtime_MusicCanBeRequestedBeforeInitialize()
        {
            TestRig rig =
                CreateRig(
                    Definition(
                        AudioId.MainMenuMusic,
                        category: AudioCategory.Music,
                        lifetime: AudioLifetimeMode.Looping,
                        spatial: AudioSpatialMode.TwoD));

            bool accepted =
                rig.Runtime.PlayMusic(
                    AudioId.MainMenuMusic);

            Assert.That(accepted, Is.True);

            rig.Initialize();

            Assert.That(
                rig.Music.CurrentMusic,
                Is.EqualTo(AudioId.MainMenuMusic));
        }

        [Test]
        public void Runtime_PlayMusic_UsesDedicatedMusicPort()
        {
            TestRig rig =
                CreateInitializedRig(
                    Definition(
                        AudioId.MainMenuMusic,
                        category: AudioCategory.Music,
                        lifetime: AudioLifetimeMode.Looping,
                        spatial: AudioSpatialMode.TwoD));

            bool result =
                rig.Runtime.PlayMusic(
                    AudioId.MainMenuMusic);

            Assert.That(result, Is.True);
            Assert.That(rig.Pool.AcquireCount, Is.Zero);
            Assert.That(
                rig.Music.CurrentMusic,
                Is.EqualTo(AudioId.MainMenuMusic));
        }

        [Test]
        public void Runtime_StopMusic_DoesNotCancelWorldSfx()
        {
            TestRig rig =
                CreateInitializedRig(
                    new[]
                    {
                        Definition(AudioId.PistolFire),
                        Definition(
                            AudioId.GameplayMusic,
                            category: AudioCategory.Music,
                            lifetime: AudioLifetimeMode.Looping,
                            spatial: AudioSpatialMode.TwoD)
                    });

            Play(rig, AudioId.PistolFire);
            rig.Runtime.PlayMusic(AudioId.GameplayMusic);

            rig.Runtime.StopMusic();

            Assert.That(rig.Model.ActiveVoiceCount, Is.EqualTo(1));
        }

        [Test]
        public void Runtime_CancelAll_StopsMusicAndSfx()
        {
            TestRig rig =
                CreateInitializedRig(
                    new[]
                    {
                        Definition(AudioId.PistolFire),
                        Definition(
                            AudioId.GameplayMusic,
                            category: AudioCategory.Music,
                            lifetime: AudioLifetimeMode.Looping,
                            spatial: AudioSpatialMode.TwoD)
                    });

            Play(rig, AudioId.PistolFire);
            rig.Runtime.PlayMusic(AudioId.GameplayMusic);

            rig.Runtime.CancelAll();

            Assert.That(rig.Model.ActiveVoiceCount, Is.Zero);
            Assert.That(rig.Music.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void ModePolicy_UiAllowedDuringSuspended()
        {
            var policy =
                new AudioModePolicy();

            AudioDefinition definition =
                Definition(
                    AudioId.UIButtonClick,
                    category: AudioCategory.UI,
                    spatial: AudioSpatialMode.TwoD);

            Assert.That(
                policy.CanPlay(
                    WorldAudioMode.Suspended,
                    in definition),
                Is.True);
        }

        [Test]
        public void ModePolicy_MusicRejectedFromSfxController()
        {
            var policy =
                new AudioModePolicy();

            AudioDefinition definition =
                Definition(
                    AudioId.MainMenuMusic,
                    category: AudioCategory.Music,
                    spatial: AudioSpatialMode.TwoD);

            Assert.That(
                policy.CanPlay(
                    WorldAudioMode.Playing,
                    in definition),
                Is.False);
        }

        [Test]
        public void ConcurrencyPolicy_AtLimitRejects()
        {
            var policy =
                new AudioConcurrencyPolicy();

            AudioDefinition definition =
                Definition(
                    AudioId.ZombieDeath,
                    maxConcurrent: 4);

            Assert.That(
                policy.CanPlay(
                    in definition,
                    4),
                Is.False);
        }

        [Test]
        public void AudioPreferences_ChannelVolumes_AreIndependent()
        {
            var preferences =
                new AudioPreferences
                {
                    MasterVolume = 1f,
                    MusicVolume = 0.2f,
                    SFXVolume = 0.4f,
                    UIVolume = 0.7f
                };

            Assert.That(
                preferences.GetCategoryVolume(AudioCategory.Music),
                Is.EqualTo(0.2f));

            Assert.That(
                preferences.GetCategoryVolume(AudioCategory.SFX),
                Is.EqualTo(0.4f));

            Assert.That(
                preferences.GetCategoryVolume(AudioCategory.UI),
                Is.EqualTo(0.7f));
        }

        [TestCase(AudioId.PistolFire)]
        [TestCase(AudioId.AKFire)]
        [TestCase(AudioId.ShotgunFire)]
        [TestCase(AudioId.SniperFire)]
        [TestCase(AudioId.GrenadeFire)]
        [TestCase(AudioId.FlamethrowerLoop)]
        [TestCase(AudioId.BulletImpact)]
        [TestCase(AudioId.GrenadeExplosion)]
        [TestCase(AudioId.ZombieHit)]
        [TestCase(AudioId.ZombieAttack)]
        [TestCase(AudioId.ZombieDeath)]
        [TestCase(AudioId.BossSpawn)]
        [TestCase(AudioId.BossAttack)]
        [TestCase(AudioId.BossHit)]
        [TestCase(AudioId.BossDeath)]
        [TestCase(AudioId.SoldierDamage)]
        [TestCase(AudioId.SoldierGroupLevelUp)]
        [TestCase(AudioId.UIButtonClick)]
        [TestCase(AudioId.WeaponSelected)]
        [TestCase(AudioId.GameOver)]
        [TestCase(AudioId.LevelComplete)]
        [TestCase(AudioId.EndGame)]
        [TestCase(AudioId.MainMenuMusic)]
        [TestCase(AudioId.GameplayMusic)]
        public void AudioId_AllProductionIds_AreDefined(
            AudioId id)
        {
            Assert.That(id, Is.Not.EqualTo(AudioId.None));
        }

        [TestCase(WorldAudioMode.Inactive)]
        [TestCase(WorldAudioMode.Playing)]
        [TestCase(WorldAudioMode.Suspended)]
        [TestCase(WorldAudioMode.TerminalDrain)]
        public void WorldAudioMode_AllValues_AreStable(
            WorldAudioMode mode)
        {
            Assert.That(
                Enum.IsDefined(
                    typeof(WorldAudioMode),
                    mode),
                Is.True);
        }

        [TestCase(AudioCategory.SFX)]
        [TestCase(AudioCategory.UI)]
        [TestCase(AudioCategory.Ambience)]
        [TestCase(AudioCategory.Music)]
        public void AudioCategory_AllValues_AreStable(
            AudioCategory category)
        {
            Assert.That(
                Enum.IsDefined(
                    typeof(AudioCategory),
                    category),
                Is.True);
        }

        [TestCase(AudioPriority.Low)]
        [TestCase(AudioPriority.Normal)]
        [TestCase(AudioPriority.High)]
        [TestCase(AudioPriority.Critical)]
        public void AudioPriority_AllValues_AreStable(
            AudioPriority priority)
        {
            Assert.That(
                Enum.IsDefined(
                    typeof(AudioPriority),
                    priority),
                Is.True);
        }

        private static AudioPlayResult Play(
            TestRig rig,
            AudioId id)
        {
            AudioDefinition definition =
                rig.CatalogDefinition(id);

            if (definition.SpatialMode ==
                AudioSpatialMode.ThreeD)
            {
                var point =
                    new AudioPoint(1f, 0f, 1f);

                var request =
                    new AudioRequest(
                        id,
                        in point);

                return rig.Runtime.Play(in request);
            }

            var request2 =
                new AudioRequest(id);

            return rig.Runtime.Play(in request2);
        }

        private static AudioDefinition Definition(
            AudioId id,
            AudioCategory category = AudioCategory.SFX,
            AudioLifetimeMode lifetime = AudioLifetimeMode.OneShot,
            AudioSpatialMode spatial = AudioSpatialMode.ThreeD,
            AudioPriority priority = AudioPriority.Normal,
            int maxConcurrent = 4,
            float baseVolume = 1f,
            float minPitch = 1f,
            float maxPitch = 1f,
            float minDistance = 0f,
            float maxDistance = 25f,
            bool terminalSafe = false)
        {
            return new AudioDefinition(
                id,
                category,
                lifetime,
                spatial,
                priority,
                maxConcurrent,
                baseVolume,
                minPitch,
                maxPitch,
                minDistance,
                maxDistance,
                terminalSafe);
        }

        private static TestRig CreateRig(
            AudioDefinition definition,
            int poolCapacity = 8)
        {
            return CreateRig(
                new[] { definition },
                poolCapacity);
        }

        private static TestRig CreateRig(
            AudioDefinition[] definitions,
            int poolCapacity = 8)
        {
            return new TestRig(
                definitions,
                poolCapacity);
        }

        private static TestRig CreateInitializedRig(
            AudioDefinition definition,
            int poolCapacity = 8)
        {
            TestRig rig =
                CreateRig(
                    definition,
                    poolCapacity);

            rig.Initialize();
            return rig;
        }

        private static TestRig CreateInitializedRig(
            AudioDefinition[] definitions,
            int poolCapacity = 8)
        {
            TestRig rig =
                CreateRig(
                    definitions,
                    poolCapacity);

            rig.Initialize();
            return rig;
        }

        private sealed class TestRig
        {
            private readonly Dictionary<AudioId, AudioDefinition> _definitions;

            public TestRig(
                AudioDefinition[] definitions,
                int poolCapacity)
            {
                _definitions =
                    new Dictionary<AudioId, AudioDefinition>();

                for (int i = 0; i < definitions.Length; i++)
                {
                    _definitions.Add(
                        definitions[i].Id,
                        definitions[i]);
                }

                Catalog =
                    new AudioCatalog(definitions);

                Model =
                    new AudioModel();

                Preferences =
                    new AudioPreferences();

                Random =
                    new FakeAudioRandom();

                Pool =
                    new FakeAudioVoicePool(poolCapacity);

                Music =
                    new FakeMusicPlaybackPort();

                var eventBus =
                    new EventBus();

                var controller =
                    new AudioController(
                        Model,
                        Preferences,
                        new AudioConcurrencyPolicy(),
                        new AudioModePolicy(),
                        Random,
                        eventBus);

                var musicController =
                    new MusicController(Preferences);

                Runtime =
                    new AudioRuntime(
                        Model,
                        controller,
                        musicController);

                Configurator = Runtime;
            }

            public AudioCatalog Catalog { get; }
            public AudioModel Model { get; }
            public AudioPreferences Preferences { get; }
            public FakeAudioRandom Random { get; }
            public FakeAudioVoicePool Pool { get; }
            public FakeMusicPlaybackPort Music { get; }
            public AudioRuntime Runtime { get; }
            public IAudioRuntimeConfigurator Configurator { get; }

            public void Initialize(
                WorldAudioMode mode = WorldAudioMode.Playing)
            {
                Runtime.SetWorldMode(mode);

                Configurator.Initialize(
                    Catalog,
                    Pool,
                    Music);
            }

            public AudioDefinition CatalogDefinition(AudioId id)
            {
                return _definitions[id];
            }
        }

        private sealed class FakeAudioRandom : IAudioRandom
        {
            public float Value { get; set; } = 1f;

            public float Range(
                float minInclusive,
                float maxInclusive)
            {
                if (Value < minInclusive)
                {
                    return minInclusive;
                }

                if (Value > maxInclusive)
                {
                    return maxInclusive;
                }

                return Value;
            }
        }

        private sealed class FakeAudioAnchor : IAudioAnchor
        {
            public FakeAudioAnchor(
                AudioPoint position,
                bool isValid = true)
            {
                Position = position;
                IsValid = isValid;
            }

            public bool IsValid { get; set; }
            public AudioPoint Position { get; set; }
        }

        private sealed class FakeAudioVoicePool : IAudioVoicePool
        {
            private readonly Queue<FakeAudioVoice> _available;
            private readonly List<FakeAudioVoice> _all;

            public FakeAudioVoicePool(int capacity)
            {
                _available =
                    new Queue<FakeAudioVoice>(capacity);

                _all =
                    new List<FakeAudioVoice>(capacity);

                for (int i = 0; i < capacity; i++)
                {
                    var voice =
                        new FakeAudioVoice(Return);

                    _all.Add(voice);
                    _available.Enqueue(voice);
                }
            }

            public int Capacity => _all.Count;
            public int AvailableCount => _available.Count;
            public int AcquireCount { get; private set; }

            public int TotalReleaseCount
            {
                get
                {
                    int count = 0;

                    for (int i = 0; i < _all.Count; i++)
                    {
                        count += _all[i].ReleaseCount;
                    }

                    return count;
                }
            }

            public FakeAudioVoice LastAcquired { get; private set; }

            public bool TryAcquire(out IAudioVoiceLease lease)
            {
                if (_available.Count == 0)
                {
                    lease = null;
                    return false;
                }

                FakeAudioVoice voice =
                    _available.Dequeue();

                voice.Acquire();
                LastAcquired = voice;
                AcquireCount++;

                lease = voice;
                return true;
            }

            private void Return(FakeAudioVoice voice)
            {
                _available.Enqueue(voice);
            }
        }

        private sealed class FakeAudioVoice : IAudioVoiceLease
        {
            private readonly Action<FakeAudioVoice> _return;

            private bool _released = true;
            private bool _playing;

            public FakeAudioVoice(
                Action<FakeAudioVoice> returnAction)
            {
                _return = returnAction;
            }

            public bool IsPlaying =>
                !_released && _playing;

            public bool IsPaused { get; private set; }
            public int ReleaseCount { get; private set; }
            public float Volume { get; private set; }
            public float Pitch { get; private set; }
            public AudioPoint LastPosition { get; private set; }

            public void Acquire()
            {
                _released = false;
                _playing = false;
                IsPaused = false;
            }

            public bool TryPlay(
                in AudioDefinition definition,
                in AudioRequest request,
                float volume,
                float pitch)
            {
                if (_released)
                {
                    return false;
                }

                Volume = volume;
                Pitch = pitch;

                if (request.Anchor != null)
                {
                    LastPosition =
                        request.Anchor.Position;
                }
                else if (request.HasPosition)
                {
                    LastPosition =
                        request.Position;
                }

                _playing = true;
                return true;
            }

            public void SetPaused(bool paused)
            {
                IsPaused = paused;
            }

            public void SetVolume(float volume)
            {
                Volume = volume;
            }

            public void SetPosition(
                in AudioPoint position)
            {
                LastPosition = position;
            }

            public void Stop()
            {
                _playing = false;
                IsPaused = false;
            }

            public void Release()
            {
                if (_released)
                {
                    return;
                }

                _released = true;
                _playing = false;
                IsPaused = false;
                ReleaseCount++;

                _return(this);
            }

            public void Complete()
            {
                _playing = false;
            }
        }

        private sealed class FakeMusicPlaybackPort :
            IMusicPlaybackPort
        {
            public AudioId CurrentMusic { get; private set; }
            public int PlayCount { get; private set; }
            public int StopCount { get; private set; }
            public float Volume { get; private set; }

            public bool Play(
                AudioId id,
                float fadeDuration,
                float volume)
            {
                CurrentMusic = id;
                Volume = volume;
                PlayCount++;
                return id != AudioId.None;
            }

            public void Stop(float fadeDuration)
            {
                CurrentMusic = AudioId.None;
                StopCount++;
            }

            public void SetVolume(float volume)
            {
                Volume = volume;
            }

            public void Tick(float deltaTime)
            {
            }

            public void Clear()
            {
                CurrentMusic = AudioId.None;
            }
        }
    }
}
