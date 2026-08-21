using GeneralCore.Architecture;
using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Commands
{
    public readonly struct RegisterBossDefeatedCommand : ICommand
    {
        public LevelBossObjectiveId Boss
        {
            get;
        }
        public RegisterBossDefeatedCommand(LevelBossObjectiveId boss)
        {
            Boss = boss;
        }
    }
}
