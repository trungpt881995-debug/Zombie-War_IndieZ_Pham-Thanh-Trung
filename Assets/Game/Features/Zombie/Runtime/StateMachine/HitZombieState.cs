using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public sealed class HitZombieState : IZombieState
    {
        private readonly ZombieStateContext _context;
        private float _remaining;
        public ZombieStateId Id => ZombieStateId.Hit;
        public HitZombieState(ZombieStateContext context) => _context = context;
        public void Enter()
        {
            _context.Model.SetState(Id);
            _context.Model.BeginHitReactionCooldown();
            _context.Motor.Stop();
            _context.View.SetLocomotionSpeed(0f);
            _context.View.PlayHit();
            _remaining = _context.Model.Definition.HitReactionDuration;
        }
        public void Tick(float deltaTime)
        {
            if (!_context.Model.GameplayEnabled) return;
            _remaining -= deltaTime;
            if (_remaining <= 0f) _context.ChangeState(ZombieStateId.Chase);
        }
        public void Finish()
        {
            if (_context.Model.State == Id) _context.ChangeState(ZombieStateId.Chase);
        }
        public void Exit() { }
    }
}
