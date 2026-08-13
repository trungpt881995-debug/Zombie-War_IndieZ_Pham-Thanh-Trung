namespace ZombieWar.Integration.GameState.Runtime
{
    public interface IGameStateSceneGateBinding
    {
        bool Bind(IGameStateRuntimeGateTarget target);
        bool Unbind(IGameStateRuntimeGateTarget target);
    }

    public interface IGameStateSceneGateRegistry : IGameStateSceneGateBinding
    {
        bool DesiredGameplayEnabled { get; }
        int Count { get; }
        void SetGameplayEnabled(bool enabled);
    }

    public interface IGameStateSoldierGate
    {
        void SetGameplayEnabled(bool enabled);
    }
}
