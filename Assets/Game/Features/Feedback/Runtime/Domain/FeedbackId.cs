namespace ZombieWar.Features.Feedback.Domain
{
    public enum FeedbackId
    {
        None = 0,

        PistolShot = 10,
        AKShot = 11,
        ShotgunShot = 12,
        SniperShot = 13,
        GrenadeShot = 14,
        FlamethrowerStart = 15,

        SoldierDamaged = 20,
        SoldierCriticalDamage = 21,

        GrenadeExplosion = 40,

        BossHit = 50,
        BossDefeated = 51,

        SoldierGroupLevelUp = 60,

        GameOver = 70,
        LevelComplete = 71,
        EndGame = 72
    }
}
