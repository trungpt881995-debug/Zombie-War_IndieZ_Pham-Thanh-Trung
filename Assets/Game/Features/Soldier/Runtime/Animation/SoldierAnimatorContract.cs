namespace ZombieWar.Features.Soldier.Animation
{
    public static class SoldierAnimatorContract
    {
        public const string MovementSpeed = "MovementSpeed";
        public const string AimX = "AimX";
        public const string AimY = "AimY";
        public const string HasTarget = "HasTarget";
        public const string Shoot = "Shoot";

        public const string BaseLayer = "Base Layer";
        public const string LocomotionState = "Locomotion";
        public const string LocomotionBlendTree = "LocomotionBlendTree";

        public const string UpperBodyLayer = "UpperBody";
        public const string UpperBodyIdleState = "UpperBodyIdle";
        public const string AimState = "Aim";
        public const string AimBlendTree = "AimBlendTree";
        public const string ShootState = "Shoot";
    }
}
