using System;
using ZombieWar.Features.Control.Domain;

namespace ZombieWar.Features.Control.Model
{
    public sealed class DynamicJoystickModel
    {
        private readonly JoystickSettings _settings;

        private ControlState _state = ControlState.Idle;
        private int _activePointerId = -1;
        private float _originX;
        private float _originY;

        public ControlState State => _state;
        public int ActivePointerId => _activePointerId;
        public JoystickSettings Settings => _settings;

        public DynamicJoystickModel(JoystickSettings settings)
        {
            _settings = settings;
        }

        public bool Begin(in ControlPointerSample pointer)
        {
            if (_state != ControlState.Idle)
                return false;

            _activePointerId = pointer.PointerId;
            _originX = pointer.X;
            _originY = pointer.Y;
            _state = ControlState.Tracking;
            return true;
        }

        public JoystickUpdateResult Update(in ControlPointerSample pointer)
        {
            if (_state != ControlState.Tracking || pointer.PointerId != _activePointerId)
                return JoystickUpdateResult.Rejected;

            float deltaX = pointer.X - _originX;
            float deltaY = pointer.Y - _originY;
            float distance = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

            if (distance <= float.Epsilon)
            {
                return new JoystickUpdateResult(true, 0f, 0f, MovementIntent.Zero);
            }

            float directionX = deltaX / distance;
            float directionY = deltaY / distance;

            float clampedDistance = distance > _settings.MaxRadius ? _settings.MaxRadius : distance;

            float handleX = directionX * clampedDistance;
            float handleY = directionY * clampedDistance;
            float rawMagnitude = clampedDistance / _settings.MaxRadius;

            if (rawMagnitude <= _settings.DeadZone)
            {
                return new JoystickUpdateResult(true, handleX, handleY, MovementIntent.Zero);
            }

            float adjustedMagnitude =
                (rawMagnitude - _settings.DeadZone) / (1f - _settings.DeadZone);

            adjustedMagnitude *= _settings.Sensitivity;
            if (adjustedMagnitude > 1f)
                adjustedMagnitude = 1f;

            var intent = new MovementIntent(directionX * adjustedMagnitude, directionY * adjustedMagnitude, adjustedMagnitude);

            return new JoystickUpdateResult(true, handleX, handleY, intent);
        }

        public bool End(int pointerId)
        {
            if (_state != ControlState.Tracking || pointerId != _activePointerId)
                return false;

            ResetState();
            return true;
        }

        public bool Cancel()
        {
            if (_state == ControlState.Idle)
                return false;

            ResetState();
            return true;
        }

        private void ResetState()
        {
            _state = ControlState.Idle;
            _activePointerId = -1;
            _originX = 0f;
            _originY = 0f;
        }
    }
}
