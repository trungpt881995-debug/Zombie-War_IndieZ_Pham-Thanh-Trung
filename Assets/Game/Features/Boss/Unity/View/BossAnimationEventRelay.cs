using UnityEngine;

namespace ZombieWar.Features.Boss.Unity.View
{
    /// <summary>
    /// Receives Unity Animation Events on the same GameObject as the Animator
    /// and forwards them to the BossView that owns gameplay presentation events.
    ///
    /// Unity Animation Events are dispatched to components on the animated
    /// GameObject. BossView can live on the Boss root while Animator lives on a
    /// child model, so this relay closes that hierarchy boundary explicitly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossAnimationEventRelay : MonoBehaviour
    {
        private BossView _target;

        public void Bind(BossView target)
        {
            _target = target;
        }

        private void Awake()
        {
            if (_target == null)
            {
                _target = GetComponentInParent<BossView>();
            }
        }

        public void AnimationEvent_AttackImpact()
        {
            _target?.AnimationEvent_AttackImpact();
        }

        public void AnimationEvent_AttackFinished()
        {
            _target?.AnimationEvent_AttackFinished();
        }

        public void AnimationEvent_HitFinished()
        {
            _target?.AnimationEvent_HitFinished();
        }

        public void AnimationEvent_DeathFinished()
        {
            _target?.AnimationEvent_DeathFinished();
        }
    }
}
