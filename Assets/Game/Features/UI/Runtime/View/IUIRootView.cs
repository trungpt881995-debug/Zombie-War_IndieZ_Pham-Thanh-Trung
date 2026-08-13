namespace ZombieWar.Features.UI.View
{
    public interface IUIRootView
    {
        IMainMenuView MainMenu{get;} IGameplayHudView Gameplay{get;} IPauseView Pause{get;}
        ILevelCompleteView LevelComplete{get;} IGameOverView GameOver{get;} IEndGameView EndGame{get;}
    }
}
