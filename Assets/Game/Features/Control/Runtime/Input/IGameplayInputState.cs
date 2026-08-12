using System;
using GeneralCore.UIInput;

namespace ZombieWar.Features.Control.Input
{
    public interface IGameplayInputState : IInputGate
    {
        event Action<bool> GameplayInputEnabledChanged;
    }
}
