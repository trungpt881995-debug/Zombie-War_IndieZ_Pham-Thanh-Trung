using System;

namespace ZombieWar.Features.Control.Domain
{
    public readonly struct JoystickSettings
    {
        public float DeadZone { get; }
        public float MaxRadius { get; }
        public float Sensitivity { get; }

        public JoystickSettings(float deadZone, float maxRadius, float sensitivity)
        {
            if (float.IsNaN(deadZone) || float.IsInfinity(deadZone) || deadZone < 0f || deadZone >= 1f)
                throw new ArgumentOutOfRangeException(nameof(deadZone), "DeadZone must be in [0, 1).");
            if (float.IsNaN(maxRadius) || float.IsInfinity(maxRadius) || maxRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxRadius), "MaxRadius must be > 0.");
            if (float.IsNaN(sensitivity) || float.IsInfinity(sensitivity) || sensitivity <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sensitivity), "Sensitivity must be > 0.");

            DeadZone = deadZone;
            MaxRadius = maxRadius;
            Sensitivity = sensitivity;
        }
    }
}
