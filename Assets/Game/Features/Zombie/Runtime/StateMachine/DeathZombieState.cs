using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Events;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public sealed class DeathZombieState : IZombieState
    {
        private enum DeathPhase { Animation, Dissolve, Complete }
        private readonly ZombieStateContext _context;
        private DeathPhase _phase;
        private float _remaining;
        private bool _killPublished;
        public ZombieStateId Id => ZombieStateId.Death;
        public DeathZombieState(ZombieStateContext context) => _context = context;

        public void Enter()
        {
            _context.Model.SetState(Id);
            _context.Model.SetTargetable(false);
            _context.Model.ClearTarget();
            _context.TargetRegistration.Unregister(_context.Model.EntityId);
            _context.Motor.Stop();
            _context.Motor.SetEnabled(false);
            _context.View.SetLocomotionSpeed(0f);
            _context.View.SetGameplayCollisionEnabled(false);
            ZombiePoint position = _context.View.Position;
            _context.Feedback.OnDeath(_context.Model.EntityId, in position);
            if (!_killPublished)
            {
                _context.EventBus.Publish(new ZombieKilledEvent(_context.Model.EntityId, _context.Model.LastDamageSource));
                _killPublished = true;
            }
            _context.View.PlayDeath();
            _phase = DeathPhase.Animation;
            _remaining = _context.Model.Definition.DeathDuration;
        }

        public void Tick(float deltaTime)
        {
            if (_phase == DeathPhase.Complete) return;
            _remaining -= deltaTime;
            if (_phase == DeathPhase.Animation)
            {
                if (_remaining <= 0f) BeginDissolve();
                return;
            }
            if (_phase == DeathPhase.Dissolve)
            {
                float duration = _context.Model.Definition.DissolveDuration;
                float normalized = duration <= 0f ? 1f : 1f - (_remaining / duration);
                if (normalized < 0f) normalized = 0f;
                if (normalized > 1f) normalized = 1f;
                _context.View.SetDissolveProgress(normalized);
                if (_remaining <= 0f) Complete();
            }
        }

        public void OnDeathAnimationFinished()
        {
            if (_phase == DeathPhase.Animation) BeginDissolve();
        }

        private void BeginDissolve()
        {
            _phase = DeathPhase.Dissolve;
            _remaining = _context.Model.Definition.DissolveDuration;
            _context.View.SetDissolveProgress(0f);
            if (_remaining <= 0f) Complete();
        }

        private void Complete()
        {
            if (_phase == DeathPhase.Complete || _context.Model.ReturnRequested) return;
            _phase = DeathPhase.Complete;
            _context.View.SetDissolveProgress(1f);
            _context.Model.MarkReturnRequested();
            _context.EventBus.Publish(new ZombieReleasedEvent(_context.Model.EntityId, ZombieReleaseReason.Death));
            _context.PoolReturn.Return(_context.Model.EntityId, ZombieReleaseReason.Death);
        }
        public void Exit() { }
        public void ResetForReuse() { _killPublished = false; _phase = DeathPhase.Complete; _remaining = 0f; }
    }
}
