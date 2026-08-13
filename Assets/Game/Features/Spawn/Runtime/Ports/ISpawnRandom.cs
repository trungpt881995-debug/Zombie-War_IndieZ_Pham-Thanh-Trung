namespace ZombieWar.Features.Spawn.Ports
{
    public interface ISpawnRandom { int Range(int minInclusive,int maxExclusive); float Value(); }
}
