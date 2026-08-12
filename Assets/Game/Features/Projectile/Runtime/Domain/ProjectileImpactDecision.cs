namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileImpactDecision
    {
        public ProjectileImpactAction Action { get; }
        public ProjectileEndReason EndReason { get; }

        public ProjectileImpactDecision(ProjectileImpactAction action, ProjectileEndReason endReason = ProjectileEndReason.None)
        {
            Action = action;
            EndReason = endReason;
        }
    }
}
