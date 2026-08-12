using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Control.Domain;
using ZombieWar.Features.Control.Input;
using ZombieWar.Features.Control.Model;
using ZombieWar.Features.Control.Ports;
using ZombieWar.Features.Control.View;

namespace ZombieWar.Features.Control.Controller
{
    public sealed class ControlController : IController, IDisposable
    {
        private readonly DynamicJoystickModel _model;
        private readonly IControlView _view;
        private readonly IGameplayInputState _inputState;
        private readonly IMovementIntentSink _movement;
        private bool _disposed;

        public ControlController(DynamicJoystickModel model, IControlView view, IGameplayInputState inputState, IMovementIntentSink movement)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inputState = inputState ?? throw new ArgumentNullException(nameof(inputState));
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));

            _view.PointerDownRequested += OnPointerDown;
            _view.PointerDragged += OnPointerDragged;
            _view.PointerUpRequested += OnPointerUp;
            _view.CancelRequested += OnCancelRequested;
            _inputState.GameplayInputEnabledChanged += OnGameplayInputEnabledChanged;

            _view.Hide();
        }

        public void Cancel()
        {
            _model.Cancel();
            StopAndHide();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _view.PointerDownRequested -= OnPointerDown;
            _view.PointerDragged -= OnPointerDragged;
            _view.PointerUpRequested -= OnPointerUp;
            _view.CancelRequested -= OnCancelRequested;
            _inputState.GameplayInputEnabledChanged -= OnGameplayInputEnabledChanged;

            _model.Cancel();
            StopAndHide();
        }

        private void OnPointerDown(ControlPointerSample pointer)
        {
            if (_disposed || !_inputState.GameplayInputEnabled)
                return;

            if (!_model.Begin(in pointer))
                return;

            _movement.Set(MovementIntent.Zero);
            _view.ShowAt(pointer.X, pointer.Y);
            _view.SetHandleOffset(0f, 0f);
        }

        private void OnPointerDragged(ControlPointerSample pointer)
        {
            if (_disposed)
                return;

            if (!_inputState.GameplayInputEnabled)
            {
                Cancel();
                return;
            }

            JoystickUpdateResult result = _model.Update(in pointer);
            if (!result.Accepted)
                return;

            _view.SetHandleOffset(result.HandleOffsetX, result.HandleOffsetY);
            var intent = result.Intent;
            _movement.Set(in intent);
        }

        private void OnPointerUp(int pointerId)
        {
            if (_disposed || !_model.End(pointerId))
                return;

            StopAndHide();
        }

        private void OnCancelRequested()
        {
            if (!_disposed)
                Cancel();
        }

        private void OnGameplayInputEnabledChanged(bool enabled)
        {
            if (!enabled && !_disposed)
                Cancel();
        }

        private void StopAndHide()
        {
            var zero = MovementIntent.Zero;
            _movement.Set(in zero);
            _view.SetHandleOffset(0f, 0f);
            _view.Hide();
        }
    }
}
