using System;

namespace GameplayCore.Time
{
    public interface IGameplayClock
    {
        double Time { get; }
        float DeltaTime { get; }
        bool IsPaused { get; }
    }

    public interface IGameplayClockControl
    {
        void SetPaused(bool paused);
        void Reset();
        void Advance(float unscaledDeltaTime);
    }

    public sealed class GameplayClock : IGameplayClock, IGameplayClockControl
    {
        public double Time { get; private set; }
        public float DeltaTime { get; private set; }
        public bool IsPaused { get; private set; }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            if (paused) DeltaTime = 0f;
        }

        public void Reset()
        {
            Time = 0d;
            DeltaTime = 0f;
            IsPaused = false;
        }

        public void Advance(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));
            if (IsPaused) { DeltaTime = 0f; return; }
            DeltaTime = unscaledDeltaTime;
            Time += unscaledDeltaTime;
        }
    }
}
