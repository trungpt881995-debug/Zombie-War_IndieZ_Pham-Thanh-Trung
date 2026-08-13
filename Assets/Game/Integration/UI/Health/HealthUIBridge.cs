using System; using GeneralCore.Architecture; using GameplayCore.Entities; using ZombieWar.Features.Health.Domain; using ZombieWar.Features.Health.Events; using ZombieWar.Features.UI.Presentation;
namespace ZombieWar.Integration.UI.Health
{
    public sealed class HealthUIBridge:IUIHealthBinding,IDisposable
    { private readonly HealthPresenter _presenter; private readonly IEventSubscriber _events; private IDisposable _sub; private IReadOnlyHealth _health; public bool IsBound=>_health!=null; public EntityId OwnerId{get;private set;}
      public HealthUIBridge(HealthPresenter presenter,IEventSubscriber events){_presenter=presenter;_events=events;}
      public void Start()=>_sub=_events.Subscribe<HealthChangedEvent>(OnChanged);
      public void Bind(EntityId ownerId,IReadOnlyHealth health){if(ownerId.Value<=0)throw new ArgumentOutOfRangeException(nameof(ownerId));_health=health??throw new ArgumentNullException(nameof(health));OwnerId=ownerId;_presenter.Present(health.NormalizedHealth,health.CurrentHealth,health.MaxHealth);}
      public void Unbind(EntityId ownerId){if(OwnerId!=ownerId)return;_health=null;OwnerId=default;}
      private void OnChanged(HealthChangedEvent e){if(!IsBound||e.OwnerId!=OwnerId)return;_presenter.Present(e.NormalizedHealth,e.CurrentHealth,e.MaxHealth);} public void Dispose(){_sub?.Dispose();_sub=null;_health=null;OwnerId=default;}
    }
}
