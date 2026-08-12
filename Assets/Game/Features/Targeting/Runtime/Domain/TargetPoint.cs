namespace ZombieWar.Features.Targeting.Domain
{
    /// <summary>
    /// Unity-independent world-space point used by Targeting domain code.
    /// </summary>
    public readonly struct TargetPoint
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public TargetPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
