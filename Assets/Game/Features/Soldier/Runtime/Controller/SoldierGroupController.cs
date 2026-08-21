using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Events;
using ZombieWar.Features.Soldier.Formation;
using ZombieWar.Features.Soldier.Model;
using ZombieWar.Features.Soldier.Movement;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Features.Soldier.Controller
{
    /// <summary>
    /// Group-level orchestration only: movement, gameplay enable/disable,
    /// Soldier count and formation. Kill progression remains in Level Feature.
    /// </summary>
    public sealed class SoldierGroupController : IController, ISoldierGroupRuntime
    {
        public const int MaxSoldiers = 4;

        private readonly SoldierGroupModel _model;
        private readonly ISoldierGroupView _view;
        private readonly SoldierController[] _soldiers;
        private readonly ISoldierMovementSolver _movementSolver;
        private readonly ISoldierGroupInputBuffer _inputBuffer;
        private readonly IFormationProvider _formationProvider;
        private readonly ITargetRangeProvider _targetRangeProvider;
        private readonly IEventBus _eventBus;
        private readonly SoldierSettings _settings;

        public EntityId GroupId => _model.GroupId;
        public SoldierGroupLevel Level => _model.Level;
        public int ActiveSoldierCount => _model.RequiredSoldierCount;
        public bool GameplayEnabled => _model.GameplayEnabled;

        public SoldierGroupController(
            SoldierGroupModel model,
            ISoldierGroupView view,
            SoldierController[] soldiers,
            ISoldierMovementSolver movementSolver,
            ISoldierGroupInputBuffer inputBuffer,
            IFormationProvider formationProvider,
            ITargetRangeProvider targetRangeProvider,
            IEventBus eventBus,
            in SoldierSettings settings)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _soldiers = soldiers ?? throw new ArgumentNullException(nameof(soldiers));

            if (_soldiers.Length != MaxSoldiers)
            {
                throw new ArgumentException(
                    $"SoldierGroup requires exactly {MaxSoldiers} Soldier controllers.",
                    nameof(soldiers));
            }

            for (int i = 0; i < _soldiers.Length; i++)
            {
                if (_soldiers[i] == null)
                {
                    throw new ArgumentException(
                        $"Soldier controller at index {i} is null.",
                        nameof(soldiers));
                }
            }

            _movementSolver = movementSolver ??
                throw new ArgumentNullException(nameof(movementSolver));
            _inputBuffer = inputBuffer ??
                throw new ArgumentNullException(nameof(inputBuffer));
            _formationProvider = formationProvider ??
                throw new ArgumentNullException(nameof(formationProvider));
            _targetRangeProvider = targetRangeProvider ??
                throw new ArgumentNullException(nameof(targetRangeProvider));
            _eventBus = eventBus ??
                throw new ArgumentNullException(nameof(eventBus));
            _settings = settings;

            ApplyCurrentFormation(publishAddedEvents: true);
        }

        public void Tick(float deltaTime)
        {
            if (!_model.GameplayEnabled)
                return;

            float safeDeltaTime = SanitizeDeltaTime(deltaTime);
            SoldierMoveInput input = _inputBuffer.Current;

            _model.SetMoveInput(in input);

            SoldierMovementStep movement =
                _movementSolver.Solve(in input, _settings.MoveSpeed);

            _view.Move(in movement, safeDeltaTime);

            SoldierDirection movementDirection =
                CreateMovementDirection(in input);

            float targetRange =
                SanitizeTargetRange(_targetRangeProvider.CurrentTargetRange);

            int count = _model.RequiredSoldierCount;

            for (int i = 0; i < count; i++)
            {
                _soldiers[i].Tick(
                    targetRange,
                    in movementDirection,
                    movement.NormalizedSpeed,
                    safeDeltaTime);
            }
        }

        public bool TryAdvanceTo(SoldierGroupLevel nextLevel)
        {
            SoldierGroupLevel previous = _model.Level;

            if (!_model.TryAdvanceTo(nextLevel))
                return false;

            ApplyCurrentFormation(publishAddedEvents: true);

            _eventBus.Publish(
                new SoldierGroupLevelChangedEvent(
                    _model.GroupId,
                    previous,
                    _model.Level));

            return true;
        }

        public void ResetForGameLevel()
        {
            SoldierGroupLevel previous = _model.Level;

            _inputBuffer.Clear();
            _model.Reset();

            ApplyCurrentFormation(publishAddedEvents: false);

            if (previous != _model.Level)
            {
                _eventBus.Publish(
                    new SoldierGroupLevelChangedEvent(
                        _model.GroupId,
                        previous,
                        _model.Level));
            }
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (_model.GameplayEnabled == enabled)
                return;

            _model.SetGameplayEnabled(enabled);

            if (!enabled)
            {
                _inputBuffer.Clear();

                for (int i = 0; i < _model.RequiredSoldierCount; i++)
                    _soldiers[i].StopGameplay();
            }
        }

        public SoldierGroupSnapshot Snapshot()
        {
            return _model.Snapshot();
        }

        private void ApplyCurrentFormation(bool publishAddedEvents)
        {
            FormationLayout layout =
                _formationProvider.Get(_model.Level);

            int activeCount = _model.RequiredSoldierCount;

            if (layout.Count != activeCount)
            {
                throw new InvalidOperationException(
                    $"Formation {_model.Level} exposes {layout.Count} slot(s), " +
                    $"but the Soldier Group requires {activeCount}.");
            }

            for (int i = 0; i < _soldiers.Length; i++)
            {
                SoldierController soldier = _soldiers[i];

                if (i < activeCount)
                {
                    SoldierPoint localPosition =
                        layout[i].LocalPosition;

                    if (!soldier.Active)
                    {
                        soldier.Activate(i, in localPosition);

                        if (publishAddedEvents)
                        {
                            _eventBus.Publish(
                                new SoldierAddedEvent(
                                    _model.GroupId,
                                    soldier.EntityId,
                                    i,
                                    _model.Level));
                        }
                    }
                    else
                    {
                        soldier.SetFormationPosition(
                            i,
                            in localPosition);
                    }
                }
                else
                {
                    soldier.Deactivate();
                }
            }
        }

        private static SoldierDirection CreateMovementDirection(
            in SoldierMoveInput input)
        {
            if (!input.HasInput)
                return SoldierDirection.Zero;

            float x = input.X;
            float z = input.Y;
            float sqrMagnitude = (x * x) + (z * z);

            if (sqrMagnitude <= 0.000001f)
                return SoldierDirection.Zero;

            float inverseLength =
                1f / (float)Math.Sqrt(sqrMagnitude);

            return new SoldierDirection(
                x * inverseLength,
                0f,
                z * inverseLength);
        }

        private static float SanitizeDeltaTime(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                return 0f;

            return value;
        }

        private static float SanitizeTargetRange(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                return 0f;

            return value;
        }
    }
}
