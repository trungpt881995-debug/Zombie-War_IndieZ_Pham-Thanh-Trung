using UnityEngine;

namespace ZombieWar.Features.Zombie.Unity.View
{
    /// <summary>
    /// Unity Animation Events are dispatched on the GameObject that owns Animator.
    /// ZombieView may live on the Zombie root while Animator lives on a child model,
    /// so this relay forwards clip events to the owning ZombieView.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZombieAnimationEventRelay : MonoBehaviour
    {
        private ZombieView _target;

        public void Bind(ZombieView target)
        {
            _target = target;
        }

        private void Awake()
        {
            if (_target == null)
            {
                _target = GetComponentInParent<ZombieView>();
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
