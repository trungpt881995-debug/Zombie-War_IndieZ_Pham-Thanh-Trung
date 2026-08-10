using GameplayCore.Time;

namespace GameplayCore.TestSupport
{
    public sealed class ManualGameplayClock : IGameplayClock, IGameplayClockControl
    {
        public double Time { get; private set; }
        public float DeltaTime { get; private set; }
        public bool IsPaused { get; private set; }
        public void SetPaused(bool paused) { IsPaused = paused; if (paused) DeltaTime = 0f; }
        public void Reset() { Time = 0d; DeltaTime = 0f; IsPaused = false; }
        public void Advance(float deltaTime) { if (IsPaused) { DeltaTime = 0f; return; } DeltaTime = deltaTime; Time += deltaTime; }
    }
}
