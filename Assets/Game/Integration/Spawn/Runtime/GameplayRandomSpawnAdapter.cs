using System; using GameplayCore.Random; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Integration.Spawn.Runtime
{
    public sealed class GameplayRandomSpawnAdapter : ISpawnRandom
    {
        private readonly IGameplayRandom _random; public GameplayRandomSpawnAdapter(IGameplayRandom random)=>_random=random??throw new ArgumentNullException(nameof(random));
        public int Range(int minInclusive,int maxExclusive)=>_random.Range(minInclusive,maxExclusive); public float Value()=>_random.Value();
    }
}
