using GeneralCore.Architecture;

namespace ZombieWar.Features.Boss.Commands
{
    public readonly struct SetBossGameplayEnabledCommand : ICommand
    {
        public bool Enabled
        {
            get;
        }
        public SetBossGameplayEnabledCommand(bool enabled)
        {
            Enabled = enabled;
        }
    }
}
