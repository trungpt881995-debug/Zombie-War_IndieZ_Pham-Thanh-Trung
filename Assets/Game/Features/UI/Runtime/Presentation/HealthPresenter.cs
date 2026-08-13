using ZombieWar.Features.UI.Model; using ZombieWar.Features.UI.View;
namespace ZombieWar.Features.UI.Presentation
{
    public sealed class HealthPresenter
    { private readonly GameplayHudModel _model; private IGameplayHudView _view; public HealthPresenter(GameplayHudModel model)=>_model=model;
      public void Bind(IGameplayHudView view){_view=view;Render();} public void Unbind()=>_view=null;
      public void Present(float normalized,float current,float max){_model.HealthNormalized=Clamp(normalized);_model.CurrentHealth=current<0?0:current;_model.MaxHealth=max<0?0:max;Render();}
      private void Render()=>_view?.SetHealth(_model.HealthNormalized,_model.CurrentHealth,_model.MaxHealth); private static float Clamp(float v)=>v<0?0:(v>1?1:v); }
}
