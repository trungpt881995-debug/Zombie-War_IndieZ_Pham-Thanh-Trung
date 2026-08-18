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
    public sealed class SoldierController :
        IController
    {
        private readonly SoldierModel _model;
        private readonly ISoldierView _view;
        private readonly ISoldierTargetingPort _targeting;
        private const float TargetAcquireFireDelaySeconds = 0.4f;

        private readonly ISoldierAttackPort _attack;
        private readonly SoldierSettings _settings;

        // Fire is gated only when a new target is acquired. Once the delay has
        // elapsed, WeaponFeature owns the normal fire cadence via WeaponConfig.
        private bool _hasTrackedTarget;
        private long _trackedTargetIdValue;
        private float _targetAcquireDelayRemaining;

        public EntityId EntityId => _model.EntityId;

        public bool Active => _model.Active;

        public SoldierSnapshot Snapshot() => _model.Snapshot();

        public SoldierController(SoldierModel model, ISoldierView view, ISoldierTargetingPort targeting, ISoldierAttackPort attack, in SoldierSettings settings)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            _view = view ?? throw new ArgumentNullException(nameof(view));

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
        }

        public void SetFormationPosition(int slotIndex,in SoldierPoint localPosition)
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

            _view.SetMovementSpeed(0f);
            _view.ClearAim();
        }

        public void Tick(float targetRange, float movementMagnitude, float deltaTime)
        {
            if (!_model.Active)
                return;

            float safeDeltaTime = SanitizeDeltaTime(deltaTime);
            float normalizedSpeed = Clamp01(movementMagnitude);

            _view.SetMovementSpeed(normalizedSpeed);

            SoldierPoint position = _view.Position;

            SoldierTargetInfo target = _targeting.Evaluate(
                _model.EntityId,
                in position,
                SanitizeRange(targetRange));

            if (!target.HasTarget)
            {
                _attack.ClearTarget(_model.EntityId);
                ResetTargetAcquireGate();

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

                // Explicitly close any previous Weapon fire session. The new
                // target may be aimed at immediately, but cannot fire until
                // the acquisition delay has elapsed.
                _attack.ClearTarget(_model.EntityId);
            }

            SoldierPoint targetPosition = target.Position;

            if (SoldierDirection.TryCreateNormalizedXZ(
                    in position,
                    in targetPosition,
                    out SoldierDirection direction))
            {
                _view.SetAimDirection(
                    in direction,
                    _settings.AimRotationDegreesPerSecond,
                    safeDeltaTime);
            }
            else
            {
                _view.ClearAim();
            }

            if (_targetAcquireDelayRemaining > 0f)
            {
                _targetAcquireDelayRemaining -= safeDeltaTime;

                if (_targetAcquireDelayRemaining > 0f)
                {
                    return;
                }

                _targetAcquireDelayRemaining = 0f;
            }

            _attack.Update(
                _model.EntityId,
                in target,
                safeDeltaTime);
        }

        private void ResetTargetAcquireGate()
        {
            _hasTrackedTarget = false;
            _trackedTargetIdValue = 0L;
            _targetAcquireDelayRemaining = 0f;
        }

        private static float SanitizeRange(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                return 0f;
            }

            return value;
        }

        private static float SanitizeDeltaTime(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                return 0f;
            }

            return value;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                return 0f;
            }

            return value >= 1f ? 1f : value;
        }
    }
}
