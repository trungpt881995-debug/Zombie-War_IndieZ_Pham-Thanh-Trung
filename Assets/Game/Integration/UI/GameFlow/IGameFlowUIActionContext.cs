using ZombieWar.Features.UI.Domain;
namespace ZombieWar.Integration.UI.GameFlow { public interface IGameFlowUIActionContext { UIFlowAction PendingAction{get;} UIFlowAction Consume(); } }
