namespace ZombieWar.Features.Control.Domain
{
    public readonly struct JoystickUpdateResult
    {
        public static readonly JoystickUpdateResult Rejected = new JoystickUpdateResult(false, 0f, 0f, MovementIntent.Zero);

        public bool Accepted { get; }
        public float HandleOffsetX { get; }
        public float HandleOffsetY { get; }
        public MovementIntent Intent { get; }

        public JoystickUpdateResult(bool accepted, float handleOffsetX, float handleOffsetY, MovementIntent intent)
        {
            Accepted = accepted;
            HandleOffsetX = handleOffsetX;
            HandleOffsetY = handleOffsetY;
            Intent = intent;
        }
    }
}
