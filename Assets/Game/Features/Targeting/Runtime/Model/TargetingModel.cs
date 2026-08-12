using System;
using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Model
{
    /// <summary>
    /// MVC Model: owns only retained-target state.
    /// </summary>
    public sealed class TargetingModel
    {
        private TargetHandle _currentTarget;

        public bool HasTarget { get; private set; }
        public TargetHandle CurrentTarget => _currentTarget;

        public void Acquire(in TargetHandle target)
        {
            if (target.Candidate == null)
                throw new ArgumentException("Target handle must contain a candidate.", nameof(target));

            _currentTarget = target;
            HasTarget = true;
        }

        public void Clear()
        {
            _currentTarget = default;
            HasTarget = false;
        }
    }
}
