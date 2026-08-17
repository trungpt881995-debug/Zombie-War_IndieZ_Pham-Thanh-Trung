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
        private readonly ISoldierAttackPort _attack;
        private readonly SoldierSettings _settings;

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
            _targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            _attack = attack ?? throw new ArgumentNullException(nameof(attack));
            _settings = settings;
        }

        public void Activate(
            int slotIndex,
            in SoldierPoint localPosition)
        {
            _view.SetLocalFormationPosition(in localPosition);
            _model.Activate(slotIndex);

            // Activate before touching Animator-driven presentation so an
            // initially inactive SoldierView has already executed Awake().
            _view.SetActive(true);
            _view.SetMovementSpeed(0f);
            _view.ClearAim();
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
            _view.SetMovementSpeed(0f);
            _view.ClearAim();
        }

        public void Tick(
            float targetRange,
            float movementMagnitude,
            float deltaTime)
        {
            if (!_model.Active)
                return;

            float normalizedSpeed = Clamp01(movementMagnitude);
            float safeDeltaTime = SanitizeDeltaTime(deltaTime);

            _view.SetMovementSpeed(normalizedSpeed);

            SoldierPoint position = _view.Position;

            SoldierTargetInfo target = _targeting.Evaluate(
                _model.EntityId,
                in position,
                SanitizeRange(targetRange));

            if (!target.HasTarget)
            {
                _attack.ClearTarget(_model.EntityId);
                _view.ClearAim();
                return;
            }

            SoldierPoint targetPosition = target.Position;

            // Full XYZ is required here. The old XZ-only direction always had Y=0,
            // so the upper body could not pitch toward a Zombie chest/AimPoint.
            if (SoldierDirection.TryCreateNormalized(
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

            // Keep the same target snapshot for Weapon. The current Weapon adapter
            // already forwards target.Position XYZ, so visual aim and shot target agree.
            _attack.Update(
                _model.EntityId,
                in target,
                safeDeltaTime);
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
