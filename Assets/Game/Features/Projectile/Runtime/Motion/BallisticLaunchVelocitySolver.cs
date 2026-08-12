using System;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Motion
{
    public sealed class BallisticLaunchVelocitySolver : IProjectileLaunchVelocitySolver
    {
        private readonly ProjectileVector _gravity;

        public BallisticLaunchVelocitySolver(in ProjectileVector gravity)
        {
            if (gravity.SqrMagnitude <= 0.000001f)
                throw new ArgumentException("Gravity cannot be zero.", nameof(gravity));
            _gravity = gravity;
        }

        public ProjectileMotionKind Kind => ProjectileMotionKind.Ballistic;

        public bool TrySolve(in ProjectileLaunchRequest request, out ProjectileVector velocity)
        {
            if (!request.HasTargetPoint || request.Speed <= 0f)
            {
                velocity = ProjectileVector.Zero;
                return false;
            }

            float dx = request.TargetPoint.X - request.Origin.X;
            float dz = request.TargetPoint.Z - request.Origin.Z;
            float horizontalDistance = (float)Math.Sqrt(dx * dx + dz * dz);
            float horizontalTime = horizontalDistance > 0.001f ? horizontalDistance / request.Speed : 0.1f;
            if (horizontalTime < 0.1f) horizontalTime = 0.1f;

            float vx = horizontalDistance > 0.001f ? dx / horizontalTime : 0f;
            float vz = horizontalDistance > 0.001f ? dz / horizontalTime : 0f;

            // Solve p(t)=p0+v0*t+0.5*g*t^2 for the initial Y velocity.
            float dy = request.TargetPoint.Y - request.Origin.Y;
            float vy = (dy - 0.5f * _gravity.Y * horizontalTime * horizontalTime) / horizontalTime;

            velocity = new ProjectileVector(vx, vy, vz);
            return velocity.SqrMagnitude > 0.000001f;
        }
    }
}
