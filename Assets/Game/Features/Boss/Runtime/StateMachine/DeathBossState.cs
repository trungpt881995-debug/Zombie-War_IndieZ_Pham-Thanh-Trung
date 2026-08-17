using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Events;

namespace ZombieWar.Features.Boss.StateMachine
{
    /// <summary>
    /// Boss death is animation-event driven.
    ///
    /// Entering Death immediately disables gameplay interaction and starts the
    /// death animation, but BossDefeatedEvent is intentionally NOT published
    /// until the death animation reports completion through
    /// OnDeathAnimationFinished(). This prevents Level Complete / End Game from
    /// appearing before the Boss has finished its Die animation.
    /// </summary>
    public sealed class DeathBossState : IBossState
    {
        private readonly BossStateContext _c;
        private bool _defeatPublished;

        public BossStateId Id => BossStateId.Death;

        public DeathBossState(BossStateContext c)
        {
            _c = c;
        }

        public void Enter()
        {
            _c.Model.SetState(Id);
            _c.Model.SetTargetable(false);
            _c.Model.ClearTarget();

            _c.TargetRegistration.Unregister(
                _c.Model.EntityId);

            _c.Motor.Stop();
            _c.Motor.SetEnabled(false);

            _c.View.SetLocomotionSpeed(0f);
            _c.View.SetGameplayCollisionEnabled(false);

            BossPoint point = _c.View.Position;
            _c.Feedback.OnDeath(
                _c.Model.Definition.Id,
                _c.Model.EntityId,
                in point);

            // IMPORTANT:
            // Do not publish BossDefeatedEvent here. The Level Feature listens to
            // that event and may enter LevelComplete/EndGame synchronously.
            _c.View.PlayDeath();
        }

        public void Tick(float dt)
        {
            // Intentionally no DeathDuration timer completion.
            // The death Animation Event is the source of truth for visual
            // completion. A timer could complete the Map before a longer clip.
        }

        public void OnDeathAnimationFinished()
        {
            CompleteAfterAnimation();
        }

        private void CompleteAfterAnimation()
        {
            if (_c.Model.ReturnRequested)
            {
                return;
            }

            if (!_defeatPublished)
            {
                _c.EventBus.Publish(
                    new BossDefeatedEvent(
                        _c.Model.Definition.Id,
                        _c.Model.EntityId,
                        _c.Model.LastDamageSource,
                        _c.Model.Definition.RewardScore));

                _defeatPublished = true;
            }

            _c.Model.MarkReturnRequested();

            _c.EventBus.Publish(
                new BossReleasedEvent(
                    _c.Model.Definition.Id,
                    _c.Model.EntityId,
                    BossReleaseReason.Death));

            _c.PoolReturn.Return(
                _c.Model.EntityId,
                BossReleaseReason.Death);
        }

        public void Exit()
        {
        }

        public void ResetForReuse()
        {
            _defeatPublished = false;
        }
    }
}
