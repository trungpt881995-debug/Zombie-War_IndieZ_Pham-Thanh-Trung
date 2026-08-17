using UnityEngine;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Config
{
    [CreateAssetMenu(
        fileName = "SoldierConfig",
        menuName = "Zombie War/Soldier/Soldier Config")]
    public sealed class SoldierConfig :
        ScriptableObject
    {
        [Header("Movement")]
        [SerializeField]
        [Min(0.01f)]
        private float moveSpeed = 5f;

        [Tooltip("How quickly each Soldier body turns toward the joystick movement direction. 0 = snap instantly.")]
        [SerializeField]
        [Min(0f)]
        private float moveRotationDegreesPerSecond = 720f;

        [Header("Aiming")]
        [Tooltip("Used only by an optional aim pivot. Animator-layer aiming can ignore this.")]
        [SerializeField]
        [Min(0f)]
        private float aimRotationDegreesPerSecond = 720f;

        public SoldierSettings CreateSettings()
        {
            return new SoldierSettings(
                moveSpeed,
                moveRotationDegreesPerSecond,
                aimRotationDegreesPerSecond);
        }
    }
}
