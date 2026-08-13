using System; using TMPro; using UnityEngine; using UnityEngine.UI; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Unity.Components; using ZombieWar.Features.UI.View;
namespace ZombieWar.Features.UI.Unity.Views
{
    public sealed class MainMenuView:CanvasGroupScreenView,IMainMenuView
    { [SerializeField] private Button playButton; [SerializeField] private TMP_Text titleText; [SerializeField] private Image background; public override UIScreenId ScreenId=>UIScreenId.MainMenu; public event Action PlayClicked;
      private void Awake(){if(playButton!=null)playButton.onClick.AddListener(()=>PlayClicked?.Invoke());} public void SetTitle(string title){if(titleText!=null)titleText.text=title;} public void SetBackground(Sprite sprite){if(background!=null){background.sprite=sprite;background.enabled=sprite!=null;}} }
}
