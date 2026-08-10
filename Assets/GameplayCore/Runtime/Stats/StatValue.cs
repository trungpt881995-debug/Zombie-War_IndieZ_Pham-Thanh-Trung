using System;

namespace GameplayCore.Stats
{
    public sealed class StatValue
    {
        public float Min { get; }
        public float Max { get; private set; }
        public float Current { get; private set; }
        public event Action<float, float> Changed;

        public StatValue(float max, float initial = -1f, float min = 0f)
        {
            Min = min;
            Max = Math.Max(min, max);
            Current = initial < min ? Max : Clamp(initial);
        }

        public void SetMax(float max, bool keepRatio = false)
        {
            var ratio = Max > Min ? (Current - Min) / (Max - Min) : 1f;
            Max = Math.Max(Min, max);
            Current = keepRatio ? Min + (Max - Min) * ratio : Clamp(Current);
            Changed?.Invoke(Current, Max);
        }

        public float Add(float amount)
        {
            Current = Clamp(Current + amount);
            Changed?.Invoke(Current, Max);
            return Current;
        }

        private float Clamp(float value) => Math.Max(Min, Math.Min(Max, value));
    }
}
