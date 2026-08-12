namespace ZombieWar.Features.Control.Domain
{
    public readonly struct ControlPointerSample
    {
        public int PointerId { get; }
        public float X { get; }
        public float Y { get; }

        public ControlPointerSample(int pointerId, float x, float y)
        {
            PointerId = pointerId;
            X = x;
            Y = y;
        }
    }
}
