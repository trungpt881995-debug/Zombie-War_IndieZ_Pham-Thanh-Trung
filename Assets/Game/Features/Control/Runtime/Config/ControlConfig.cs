using UnityEngine;
using ZombieWar.Features.Control.Domain;

namespace ZombieWar.Features.Control.Config
{
    [CreateAssetMenu(
        fileName = "ControlConfig",
        menuName = "Zombie War/Control/Control Config")]
    public sealed class ControlConfig : ScriptableObject
    {
        [SerializeField, Range(0f, 0.95f)]
        private float deadZone = 0.15f;

        [SerializeField, Min(1f)]
        private float maxRadius = 100f;

        [SerializeField, Min(0.01f)]
        private float sensitivity = 1f;

        public float DeadZone => deadZone;
        public float MaxRadius => maxRadius;
        public float Sensitivity => sensitivity;

        public JoystickSettings CreateSettings()
        {
            return new JoystickSettings(deadZone, maxRadius, sensitivity);
        }
    }
}
