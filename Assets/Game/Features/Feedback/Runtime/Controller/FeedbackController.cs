using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Feedback.Catalog;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Events;
using ZombieWar.Features.Feedback.Model;
using ZombieWar.Features.Feedback.Policies;
using ZombieWar.Features.Feedback.Ports;
using ZombieWar.Features.Feedback.Services;

namespace ZombieWar.Features.Feedback.Controller
{
    public sealed class FeedbackController
    {
        private readonly FeedbackModel _model;
        private readonly IEventBus _events;
        private readonly IHapticCooldownPolicy _hapticCooldown;
        private readonly IFeedbackPriorityPolicy _priority;
        private readonly IFeedbackPreferences _preferences;

        private IFeedbackCatalog _catalog;
        private ICameraFeedbackPort _camera;
        private IHapticFeedbackPort _haptic;
        private IScreenFeedbackPort _screen;
        private IRecoilFeedbackPort _recoil;

        public FeedbackController(
            FeedbackModel model,
            IEventBus events,
            IHapticCooldownPolicy hapticCooldown,
            IFeedbackPriorityPolicy priority,
            IFeedbackPreferences preferences)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _hapticCooldown = hapticCooldown ?? throw new ArgumentNullException(nameof(hapticCooldown));
            _priority = priority ?? throw new ArgumentNullException(nameof(priority));
            _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        }

        public bool IsInitialized => _model.IsInitialized;
        public FeedbackRuntimeMode Mode => _model.Mode;

        public FeedbackSnapshot Snapshot =>
            new FeedbackSnapshot(
                _model.IsInitialized,
                _model.Mode,
                _model.Elapsed,
                _model.Sequence,
                _model.AcceptedCount,
                _model.RejectedCount);

        public void Initialize(
            IFeedbackCatalog catalog,
            ICameraFeedbackPort camera,
            IHapticFeedbackPort haptic,
            IScreenFeedbackPort screen,
            IRecoilFeedbackPort recoil)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (haptic == null)
            {
                throw new ArgumentNullException(nameof(haptic));
            }

            if (screen == null)
            {
                throw new ArgumentNullException(nameof(screen));
            }

            if (recoil == null)
            {
                throw new ArgumentNullException(nameof(recoil));
            }

            if (IsInitialized)
            {
                Shutdown();
            }

            _catalog = catalog;
            _camera = camera;
            _haptic = haptic;
            _screen = screen;
            _recoil = recoil;

            _model.Initialize();
            ApplyModeOutputs(_model.Mode);
        }

        public void Shutdown()
        {
            CancelAll();

            _catalog = null;
            _camera = null;
            _haptic = null;
            _screen = null;
            _recoil = null;

            _model.Shutdown();
        }

        public FeedbackResult Play(in FeedbackRequest request)
        {
            if (!IsInitialized)
            {
                return Reject(
                    request.Id,
                    FeedbackFailure.NotInitialized);
            }

            if (_model.Mode == FeedbackRuntimeMode.Inactive ||
                _model.Mode == FeedbackRuntimeMode.Suspended)
            {
                return Reject(
                    request.Id,
                    FeedbackFailure.RuntimeModeRejected);
            }

            if (!_catalog.TryGet(
                    request.Id,
                    out FeedbackRecipe recipe))
            {
                return Reject(
                    request.Id,
                    FeedbackFailure.DefinitionNotFound);
            }

            if (_model.Mode == FeedbackRuntimeMode.TerminalDrain &&
                !recipe.AllowDuringTerminalDrain)
            {
                return Reject(
                    request.Id,
                    FeedbackFailure.TerminalRejected);
            }

            FeedbackChannel channels = FeedbackChannel.None;
            float now = _model.Elapsed;

            if (recipe.Camera.Enabled &&
                _preferences.CameraEnabled &&
                _priority.TryAcquire(
                    FeedbackChannel.Camera,
                    recipe.Priority,
                    now,
                    recipe.Camera.OccupancyDuration) &&
                _camera.TryPlay(recipe.Camera.Cue))
            {
                channels |= FeedbackChannel.Camera;
            }

            if (recipe.Haptic.Enabled &&
                _preferences.HapticEnabled &&
                _hapticCooldown.TryConsume(
                    recipe.Id,
                    now,
                    recipe.Haptic.Cooldown) &&
                _haptic.TryPlay(recipe.Haptic.Strength))
            {
                channels |= FeedbackChannel.Haptic;
            }

            if (recipe.Screen.Enabled &&
                _preferences.ScreenEnabled &&
                _priority.TryAcquire(
                    FeedbackChannel.Screen,
                    recipe.Priority,
                    now,
                    recipe.Screen.Duration) &&
                _screen.TryFlash(
                    recipe.Screen.Kind,
                    Clamp01(recipe.Screen.Intensity * request.Intensity),
                    recipe.Screen.Duration))
            {
                channels |= FeedbackChannel.Screen;
            }

            if (recipe.Recoil.Enabled &&
                _preferences.RecoilEnabled &&
                _priority.TryAcquire(
                    FeedbackChannel.Recoil,
                    recipe.Priority,
                    now,
                    recipe.Recoil.Duration) &&
                _recoil.TryApply(
                    recipe.Recoil.Strength * request.Intensity,
                    recipe.Recoil.Duration))
            {
                channels |= FeedbackChannel.Recoil;
            }

            long sequence = _model.RecordAccepted();

            _events.Publish(
                new FeedbackPlayedEvent(
                    recipe.Id,
                    channels,
                    sequence));

            return FeedbackResult.Accept(
                recipe.Id,
                channels,
                sequence);
        }

        public void SetMode(FeedbackRuntimeMode mode)
        {
            if (mode < FeedbackRuntimeMode.Inactive ||
                mode > FeedbackRuntimeMode.TerminalDrain)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            if (_model.Mode == mode)
            {
                return;
            }

            _model.SetMode(mode);

            if (IsInitialized)
            {
                ApplyModeOutputs(mode);
            }
        }

        public void Tick(float deltaTime)
        {
            _model.Tick(deltaTime);
        }

        public void CancelAll()
        {
            _camera?.CancelAll();
            _haptic?.CancelAll();
            _screen?.Clear();
            _recoil?.CancelAll();

            _hapticCooldown.Reset();
            _priority.Reset();
        }

        private FeedbackResult Reject(
            FeedbackId id,
            FeedbackFailure failure)
        {
            _model.RecordRejected();

            _events.Publish(
                new FeedbackRejectedEvent(
                    id,
                    failure));

            return FeedbackResult.Reject(
                id,
                failure,
                _model.Sequence);
        }

        private void ApplyModeOutputs(FeedbackRuntimeMode mode)
        {
            switch (mode)
            {
                case FeedbackRuntimeMode.Inactive:
                    CancelAll();
                    _screen.SetSuspended(false);
                    break;

                case FeedbackRuntimeMode.Suspended:
                    _screen.SetSuspended(true);
                    break;

                case FeedbackRuntimeMode.Playing:
                case FeedbackRuntimeMode.TerminalDrain:
                    _screen.SetSuspended(false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return value >= 1f ? 1f : value;
        }
    }
}
