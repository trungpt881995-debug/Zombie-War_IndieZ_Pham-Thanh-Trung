namespace ZombieWar.Features.Zombie.Domain
{
    public readonly struct ZombieSpawnRequest
    {
        public ZombiePoint Position { get; }
        public ZombieSpawnRequest(in ZombiePoint position) => Position = position;
    }
}
