using GeneralCore.Architecture;
using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.View
{
    public interface IGameFlowView : IView
    {
        void Render(GameFlowStateId state);
    }

    public sealed class NullGameFlowView : IGameFlowView
    {
        public void Render(GameFlowStateId state) { }
    }
}
