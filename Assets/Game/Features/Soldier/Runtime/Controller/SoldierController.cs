using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Model;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Features.Soldier.Controller
{
    /// <summary>
    /// Per-Soldier orchestration: presentation + Targeting port + Attack port.
    /// It never searches Zombie, owns Weapon FireRate, spawns Projectile or applies Damage.
    /// </summary>
    public sealed class SoldierController : IController
    {
        private const float TargetAcquireFireDelaySeconds = 0.4f;
        private const float DirectionEpsilon = 0.000001f;
        private const float RadToDeg = 57.29577951308232f;

        private readonly SoldierModel _model;
        private readonly ISoldierView _view;
        private readonly ISoldierFacingView _facingView;
        private readonly ISoldierTargetingPort _targeting;
        private readonly ISoldierAttackPort _attack;
        private readonly SoldierSettings _settings;

        // Fire is gated only when a new target is acquired. Once the delay has
        // elapsed, WeaponFeature owns the normal fire cadence via WeaponConfig.
        private bool _hasTrackedTarget;
        private long _trackedTargetIdValue;
        private float _targetAcquireDelayRemaining;

        // Hysteresis state. While true, whole-body facing is target-owned and the
        // same joystick movement is presented with negative MovementSpeed.
        private bool _targetFacingMode;

        public EntityId EntityId => _model.EntityId;
        public bool Active => _model.Active;
        public SoldierSnapshot Snapshot() => _model.Snapshot();

        public SoldierController(
            SoldierModel model,
            ISoldierView view,
            ISoldierTargetingPort targeting,
            ISoldierAttackPort attack,
            in SoldierSettings settings)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _facingView = view as ISoldierFacingView;
            _targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            _attack = attack ?? throw new ArgumentNullException(nameof(attack));
            _settings = settings;
        }

        public void Activate(int slotIndex, in SoldierPoint localPosition)
        {
            _view.SetLocalFormationPosition(in localPosition);
            _model.Activate(slotIndex);

            // Activate before touching Animator-driven presentation so an
            // initially inactive SoldierView has already executed Awake().
            _view.SetActive(true);
            _view.SetMovementSpeed(0f);
            _view.ClearAim();
            ResetTargetAcquireGate();
            _targetFacingMode = false;
        }

        public void SetFormationPosition(
            int slotIndex,
            in SoldierPoint localPosition)
        {
            _model.SetSlot(slotIndex);
            _view.SetLocalFormationPosition(in localPosition);
        }

        public void Deactivate()
        {
            if (_model.Active)
                StopGameplay();

            _model.Deactivate();

            // The view may have been authored active in the scene even while
            // the fresh SoldierModel starts inactive.
            _view.SetActive(false);
        }

        public void StopGameplay()
        {
            _targeting.Clear(_model.EntityId);
            _attack.ClearTarget(_model.EntityId);
            ResetTargetAcquireGate();
            _targetFacingMode = false;

            _view.SetMovementSpeed(0f);
            _view.ClearAim();
        }

        /// <summary>
        /// Backward-compatible overload for tests/tools that do not provide a
        /// movement direction. Production group runtime uses the overload below.
        /// </summary>
        public void Tick(
            float targetRange,
            float movementMagnitude,
            float deltaTime)
        {
            SoldierDirection movementDirection = SoldierDirection.Zero;
            Tick(
                targetRange,
                in movementDirection,
                movementMagnitude,
                deltaTime);
        }

        public void Tick(
            float targetRange,
            in SoldierDirection movementDirection,
            float movementMagnitude,
            float deltaTime)
        {
            if (!_model.Active)
                return;

            float safeDeltaTime = SanitizeDeltaTime(deltaTime);
            float normalizedSpeed = Clamp01(movementMagnitude);
            bool isMoving =
                normalizedSpeed > 0f &&
                movementDirection.HasDirection;

            SoldierPoint position = _view.Position;

            SoldierTargetInfo target = _targeting.Evaluate(
                _model.EntityId,
                in position,
                SanitizeRange(targetRange));

            if (!target.HasTarget)
            {
                _attack.ClearTarget(_model.EntityId);
                ResetTargetAcquireGate();
                _targetFacingMode = false;

                ApplyNoTargetLocomotion(
                    in movementDirection,
                    normalizedSpeed,
                    safeDeltaTime);

                _view.ClearAim();
                return;
            }

            bool acquiredNewTarget =
                !_hasTrackedTarget ||
                _trackedTargetIdValue != target.TargetId.Value;

            if (acquiredNewTarget)
            {
                _hasTrackedTarget = true;
                _trackedTargetIdValue = target.TargetId.Value;
                _targetAcquireDelayRemaining =
                    TargetAcquireFireDelaySeconds;

                // Re-evaluate body-facing from the new target instead of carrying
                // the previous target's combat-facing state across the switch.
                _targetFacingMode = false;

                // Explicitly close any previous Weapon fire session. The new
                // target may be aimed at immediately, but cannot fire until
                // the acquisition delay has elapsed.
                _attack.ClearTarget(_model.EntityId);
            }

            SoldierPoint targetPosition = target.Position;

            if (SoldierDirection.TryCreateNormalizedXZ(
                    in position,
                    in targetPosition,
                    out SoldierDirection targetDirection))
            {
                ApplyTargetFacingAndLocomotion(
                    in movementDirection,
                    in targetDirection,
                    normalizedSpeed,
                    isMoving,
                    safeDeltaTime);

                // Body facing is applied first. AimX/AimY are then calculated in
                // the Soldier's updated local space, keeping upper-body aim aligned.
                _view.SetAimDirection(
                    in targetDirection,
                    _settings.AimRotationDegreesPerSecond,
                    safeDeltaTime);
            }
            else
            {
                _targetFacingMode = false;
                ApplyNoTargetLocomotion(
                    in movementDirection,
                    normalizedSpeed,
                    safeDeltaTime);
                _view.ClearAim();
            }

            if (_targetAcquireDelayRemaining > 0f)
            {
                _targetAcquireDelayRemaining -= safeDeltaTime;

                if (_targetAcquireDelayRemaining > 0f)
                    return;

                _targetAcquireDelayRemaining = 0f;
            }

            _attack.Update(
                _model.EntityId,
                in target,
                safeDeltaTime);
        }

        private void ApplyNoTargetLocomotion(
            in SoldierDirection movementDirection,
            float normalizedSpeed,
            float deltaTime)
        {
            if (_facingView != null && movementDirection.HasDirection)
            {
                _facingView.SetBodyFacing(
                    in movementDirection,
                    _settings.MoveRotationDegreesPerSecond,
                    deltaTime);
            }

            _view.SetMovementSpeed(normalizedSpeed);
        }

        private void ApplyTargetFacingAndLocomotion(
            in SoldierDirection movementDirection,
            in SoldierDirection targetDirection,
            float normalizedSpeed,
            bool isMoving,
            float deltaTime)
        {
            if (_facingView == null)
            {
                // Preserve old presentation behavior for mocks/alternate views that
                // have not opted into the body-facing capability.
                _view.SetMovementSpeed(normalizedSpeed);
                return;
            }

            UpdateTargetFacingMode(
                in movementDirection,
                in targetDirection,
                isMoving);

            if (_targetFacingMode)
            {
                _facingView.SetBodyFacing(
                    in targetDirection,
                    _settings.MoveRotationDegreesPerSecond,
                    deltaTime);
            }
            else if (isMoving)
            {
                _facingView.SetBodyFacing(
                    in movementDirection,
                    _settings.MoveRotationDegreesPerSecond,
                    deltaTime);
            }

            float signedSpeed =
                _targetFacingMode && isMoving
                    ? -normalizedSpeed
                    : normalizedSpeed;

            _view.SetMovementSpeed(signedSpeed);
        }

        private void UpdateTargetFacingMode(
            in SoldierDirection movementDirection,
            in SoldierDirection targetDirection,
            bool isMoving)
        {
            if (isMoving)
            {
                // Important: use movement-vs-target angle while moving. If we used
                // current body-vs-target after turning, the angle would immediately
                // collapse toward zero and cause target-facing oscillation.
                float angle = AngleDegreesXZ(
                    in movementDirection,
                    in targetDirection);

                if (_targetFacingMode)
                {
                    if (angle <= _settings.BodyTurnReleaseAimAngleDegrees)
                        _targetFacingMode = false;
                }
                else if (angle > _settings.BodyTurnEnterAimAngleDegrees)
                {
                    _targetFacingMode = true;
                }

                return;
            }

            // With no joystick direction there is no movement-facing reference.
            // A fresh target outside the current body's aim cone can still turn the
            // whole Soldier. Once entered while idle, keep target-facing until the
            // target changes/disappears or movement resumes; this avoids ping-pong.
            if (_targetFacingMode)
                return;

            SoldierDirection currentForward = _facingView.Forward;
            if (!currentForward.HasDirection)
                return;

            float idleTargetAngle = AngleDegreesXZ(
                in currentForward,
                in targetDirection);

            if (idleTargetAngle > _settings.BodyTurnEnterAimAngleDegrees)
                _targetFacingMode = true;
        }

        private void ResetTargetAcquireGate()
        {
            _hasTrackedTarget = false;
            _trackedTargetIdValue = 0L;
            _targetAcquireDelayRemaining = 0f;
        }

        private static float AngleDegreesXZ(
            in SoldierDirection a,
            in SoldierDirection b)
        {
            float aSqr = (a.X * a.X) + (a.Z * a.Z);
            float bSqr = (b.X * b.X) + (b.Z * b.Z);

            if (aSqr <= DirectionEpsilon || bSqr <= DirectionEpsilon)
                return 0f;

            float denominator =
                (float)Math.Sqrt(aSqr * bSqr);

            float dot =
                ((a.X * b.X) + (a.Z * b.Z)) /
                denominator;

            if (dot < -1f) dot = -1f;
            else if (dot > 1f) dot = 1f;

            return (float)Math.Acos(dot) * RadToDeg;
        }

        private static float SanitizeRange(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                return 0f;

            return value;
        }

        private static float SanitizeDeltaTime(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                return 0f;

            return value;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                return 0f;

            return value >= 1f ? 1f : value;
        }
    }
}
