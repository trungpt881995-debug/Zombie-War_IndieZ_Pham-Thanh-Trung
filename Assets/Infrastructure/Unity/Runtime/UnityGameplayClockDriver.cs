using GameplayCore.Time;
using UnityEngine;
using VContainer.Unity;

namespace ZombieWar.Infrastructure.Unity
{
    public sealed class UnityGameplayClockDriver : ITickable
    {
        private readonly IGameplayClockControl _clock;
        public UnityGameplayClockDriver(IGameplayClockControl clock) => _clock = clock;
        public void Tick() => _clock.Advance(Time.unscaledDeltaTime);
    }
}
