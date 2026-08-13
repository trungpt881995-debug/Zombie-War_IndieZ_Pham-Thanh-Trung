using ZombieWar.Features.UI.Domain;
namespace ZombieWar.Features.UI.View { public interface IUIScreenView { UIScreenId ScreenId{get;} void SetVisible(bool visible); } }
