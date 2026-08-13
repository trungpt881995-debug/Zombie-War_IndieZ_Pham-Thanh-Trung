using System; using TMPro; using UnityEngine; using UnityEngine.UI; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Unity.Components; using ZombieWar.Features.UI.Unity.Config; using ZombieWar.Features.UI.View;
namespace ZombieWar.Features.UI.Unity.Views
{
    public sealed class GameplayHudView:CanvasGroupScreenView,IGameplayHudView
    { [SerializeField] private TMP_Text scoreText; [SerializeField] private TMP_Text levelText; [SerializeField] private TMP_Text groupLevelText; [SerializeField] private TMP_Text healthText; [SerializeField] private Image healthFill; [SerializeField] private Button pauseButton; [SerializeField] private WeaponButtonView[] weaponButtons=new WeaponButtonView[0];
      public override UIScreenId ScreenId=>UIScreenId.Gameplay; public event Action PauseClicked; public event Action<UIWeaponId> WeaponClicked;
      private void Awake(){if(pauseButton!=null)pauseButton.onClick.AddListener(()=>PauseClicked?.Invoke());for(int i=0;i<weaponButtons.Length;i++)if(weaponButtons[i]!=null)weaponButtons[i].Clicked+=OnWeapon;} private void OnDestroy(){for(int i=0;i<weaponButtons.Length;i++)if(weaponButtons[i]!=null)weaponButtons[i].Clicked-=OnWeapon;} private void OnWeapon(UIWeaponId id)=>WeaponClicked?.Invoke(id);
      public void SetScore(long s){if(scoreText!=null)scoreText.text=$"SCORE  {s:N0}";} public void SetGameLevel(int l){if(levelText!=null)levelText.text=l>0?$"LEVEL {l}":"LEVEL -";} public void SetSoldierGroupLevel(int l){if(groupLevelText!=null)groupLevelText.text=l>0?$"GROUP Lv.{l}":"GROUP Lv.-";}
      public void SetHealth(float n,float c,float m){if(healthFill!=null)healthFill.fillAmount=Mathf.Clamp01(n);if(healthText!=null)healthText.text=m>0f?$"{Mathf.CeilToInt(c)} / {Mathf.CeilToInt(m)}":"-- / --";}
      public void SetWeaponState(UIWeaponId id,bool selected,float cooldown,bool interactable){for(int i=0;i<weaponButtons.Length;i++)if(weaponButtons[i]!=null&&weaponButtons[i].Weapon==id){weaponButtons[i].Render(selected,cooldown,interactable);return;}}
      public void ApplyWeaponConfig(WeaponUIConfig config){if(config==null)return;for(int i=0;i<weaponButtons.Length;i++)if(weaponButtons[i]!=null&&config.TryGet(weaponButtons[i].Weapon,out var e))weaponButtons[i].Apply(string.IsNullOrEmpty(e.displayName)?e.weapon.ToString():e.displayName,e.icon);} }
}
