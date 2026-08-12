using System;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Events;
using ZombieWar.Features.Zombie.Model;
using ZombieWar.Features.Zombie.Ports;
using ZombieWar.Features.Zombie.StateMachine;

namespace ZombieWar.Features.Zombie.Controller
{
    public sealed class ZombieController : IController
    {
        private readonly IEntityIdGenerator _idGenerator;
        private readonly ZombieModel _model;
        private readonly IZombieView _view;
        private readonly IZombieMotor _motor;
        private readonly IZombieHealthPort _health;
        private readonly IZombieTargetRegistrationPort _targetRegistration;
        private readonly IZombiePoolReturnPort _poolReturn;
        private readonly IZombieFeedbackPort _feedback;
        private readonly IEventBus _eventBus;
        private readonly ZombieStateMachine _stateMachine;
        private readonly AttackZombieState _attackState;
        private readonly HitZombieState _hitState;
        private readonly DeathZombieState _deathState;

        public EntityId EntityId => _model.EntityId;
        public ZombieStateId State => _model.State;
        public bool IsActive => _model.IsActive;
        public bool IsAlive => _model.IsActive && _model.State != ZombieStateId.Death && _health.IsAlive;
        public bool IsTargetable => IsAlive && _model.IsTargetable;
        public bool GameplayEnabled => _model.GameplayEnabled;
        public ZombiePoint Position => _view.Position;
        public float CurrentHealth => _health.CurrentHealth;
        public float MaxHealth => _health.MaxHealth;

        public ZombieController(
            IEntityIdGenerator idGenerator,
            ZombieModel model,
            IZombieView view,
            IZombieMotor motor,
            IZombieHealthPort health,
            IZombieTargetProvider targetProvider,
            IZombieAttackPort attackPort,
            IZombieTargetRegistrationPort targetRegistration,
            IZombiePoolReturnPort poolReturn,
            IZombieFeedbackPort feedback,
            IEventBus eventBus)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _targetRegistration = targetRegistration ?? throw new ArgumentNullException(nameof(targetRegistration));
            _poolReturn = poolReturn ?? throw new ArgumentNullException(nameof(poolReturn));
            _feedback = feedback ?? NullZombieFeedbackPort.Instance;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            var context = new ZombieStateContext(
                _model, _view, _motor,
                targetProvider ?? throw new ArgumentNullException(nameof(targetProvider)),
                attackPort ?? throw new ArgumentNullException(nameof(attackPort)),
                _targetRegistration, _poolReturn, _feedback, _eventBus, ChangeState);

            _stateMachine = new ZombieStateMachine();
            _stateMachine.Register(new SpawnZombieState(context));
            _stateMachine.Register(new ChaseZombieState(context));
            _attackState = new AttackZombieState(context);
            _hitState = new HitZombieState(context);
            _deathState = new DeathZombieState(context);
            _stateMachine.Register(_attackState);
            _stateMachine.Register(_hitState);
            _stateMachine.Register(_deathState);
        }

        public EntityId Activate(in ZombieDefinition definition, in ZombieSpawnRequest request)
        {
            if (_model.IsActive) throw new InvalidOperationException("Zombie is already active.");
            EntityId id = _idGenerator.Next();
            _model.Activate(id, in definition);
            _deathState.ResetForReuse();
            _health.Initialize(id, definition.MaxHealth);
            _view.ResetForReuse();
            _view.SetActive(true);
            _view.SetAnimationPaused(false);
            _view.SetGameplayCollisionEnabled(true);
            ZombiePoint spawnPosition = request.Position;
            _motor.Warp(in spawnPosition);
            _motor.SetEnabled(true);
            _targetRegistration.Register(id);
            _eventBus.Publish(new ZombieActivatedEvent(id));
            _stateMachine.Change(ZombieStateId.Spawn);
            return id;
        }

        public void Tick(float deltaTime)
        {
            if (!_model.IsActive || deltaTime < 0f) return;
            if (!_model.GameplayEnabled) return;
            _model.TickTimers(deltaTime);
            _stateMachine.Tick(deltaTime);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (!_model.IsActive || _model.GameplayEnabled == enabled) return;
            _model.SetGameplayEnabled(enabled);
            _view.SetAnimationPaused(!enabled);
            _motor.SetEnabled(enabled && _model.State != ZombieStateId.Death);
            if (!enabled)
            {
                _motor.Stop();
                _view.SetLocomotionSpeed(0f);
            }
        }

        public void ReceiveDamage(DamageInfo damage)
        {
            if (!IsAlive || damage.Amount <= 0f) return;
            _model.SetLastDamageSource(damage.Source);
            _health.ApplyDamage(damage);
            if (!_health.IsAlive)
            {
                ChangeState(ZombieStateId.Death);
                return;
            }

            ZombiePoint position = _view.Position;
            _feedback.OnHit(_model.EntityId, in position);
            if (_model.CanStartHitReaction && _model.State != ZombieStateId.Spawn && _model.State != ZombieStateId.Death)
                ChangeState(ZombieStateId.Hit);
        }

        public void NotifyAttackImpact()
        {
            if (_model.State == ZombieStateId.Attack) _attackState.OnAttackImpact();
        }
        public void NotifyAttackAnimationFinished()
        {
            if (_model.State == ZombieStateId.Attack) _attackState.OnAttackAnimationFinished();
        }
        public void NotifyHitAnimationFinished()
        {
            if (_model.State == ZombieStateId.Hit) _hitState.Finish();
        }
        public void NotifyDeathAnimationFinished()
        {
            if (_model.State == ZombieStateId.Death) _deathState.OnDeathAnimationFinished();
        }

        public void Cancel()
        {
            if (!_model.IsActive || _model.ReturnRequested) return;
            EntityId id = _model.EntityId;
            _model.SetTargetable(false);
            _model.ClearTarget();
            _targetRegistration.Unregister(id);
            _motor.Stop();
            _motor.SetEnabled(false);
            _view.SetGameplayCollisionEnabled(false);
            _model.MarkReturnRequested();
            _eventBus.Publish(new ZombieReleasedEvent(id, ZombieReleaseReason.Cancelled));
            _poolReturn.Return(id, ZombieReleaseReason.Cancelled);
        }

        public void DeactivateForPool()
        {
            if (!_model.IsActive) return;
            EntityId id = _model.EntityId;
            _targetRegistration.Unregister(id);
            _stateMachine.Clear();
            _motor.Stop();
            _motor.SetEnabled(false);
            _view.SetGameplayCollisionEnabled(false);
            _view.SetAnimationPaused(false);
            _view.SetActive(false);
            _model.Deactivate();
        }

        private void ChangeState(ZombieStateId id)
        {
            if (!_model.IsActive || _model.State == ZombieStateId.Death && id != ZombieStateId.Death) return;
            _stateMachine.Change(id);
        }
    }
}
