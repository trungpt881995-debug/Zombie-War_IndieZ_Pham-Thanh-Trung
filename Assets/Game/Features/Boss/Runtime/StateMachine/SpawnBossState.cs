using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class SpawnBossState : IBossState
    {
        private readonly BossStateContext _c;
        private float _remaining;
        public BossStateId Id => BossStateId.Spawn;
        public SpawnBossState(BossStateContext c)
        {
            _c = c;
        }
        public void Enter()
        {
            _c.Model.SetState(Id);
            _c.Motor.Stop();
            _c.View.SetLocomotionSpeed(0f);
            _c.View.PlaySpawn();
            BossPoint p = _c.View.Position;
            _c.Feedback.OnSpawn(_c.Model.Definition.Id, _c.Model.EntityId, in p);
            _remaining = _c.Model.Definition.SpawnDuration;
            if (_remaining <= 0f) _c.ChangeState(BossStateId.Chase);
        }
        public void Tick(float dt)
        {
            if (!_c.Model.GameplayEnabled) return;
            _remaining -= dt;
            if (_remaining <= 0f) _c.ChangeState(BossStateId.Chase);
        }
        public void Exit()
        {
        }
    }
}
