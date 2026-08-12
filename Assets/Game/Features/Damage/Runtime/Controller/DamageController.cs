using System;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using ZombieWar.Features.Damage.Domain;
using ZombieWar.Features.Damage.Events;
using ZombieWar.Features.Damage.Model;
using ZombieWar.Features.Damage.View;

namespace ZombieWar.Features.Damage.Controller
{
    /// <summary>
    /// MVC Controller and Zombie War implementation of Gameplay Core IDamageService.
    /// Orchestrates Model -> target port -> presentation/event notification.
    /// </summary>
    public sealed class DamageController : IController, IDamageService
    {
        private readonly DamageModel _model;
        private readonly IDamageView _view;
        private readonly IEventBus _eventBus;

        public DamageController(DamageModel model, IDamageView view, IEventBus eventBus)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? NullDamageView.Instance;
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public bool TryApply(IDamageable target, DamageInfo damage)
        {
            DamageResolution result = _model.Resolve(target, in damage);

            Render(in result);

            if (!result.Accepted)
            {
                return false;
            }

            var resolvedDamage = new DamageInfo(result.SourceId, result.FinalAmount, result.Type);

            target.ApplyDamage(resolvedDamage);

            _eventBus.Publish(new DamageAppliedEvent(result.SourceId,result.TargetId, result.FinalAmount, result.Type));

            return true;
        }

        private void Render(in DamageResolution result)
        {
            var state = new DamageViewState(in result);
            _view.Render(in state);
        }
    }
}
