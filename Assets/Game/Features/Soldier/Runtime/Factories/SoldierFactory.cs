using System;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Controller;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Model;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Features.Soldier.Factories
{
    public sealed class SoldierFactory :
        ISoldierFactory
    {
        private readonly IEntityIdGenerator _ids;
        private readonly ISoldierTargetingPort _targeting;
        private readonly ISoldierAttackPort _attack;

        public SoldierFactory(IEntityIdGenerator ids, ISoldierTargetingPort targeting, ISoldierAttackPort attack)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));

            _targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));

            _attack = attack ?? throw new ArgumentNullException(nameof(attack));
        }

        public SoldierController Create( int slotIndex, ISoldierView view, in SoldierSettings settings)
        {
            if (slotIndex < 0 || slotIndex >= SoldierGroupController.MaxSoldiers)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var model = new SoldierModel(_ids.Next());

            model.SetSlot(slotIndex);

            return new SoldierController(model, view, _targeting, _attack, in settings);
        }
    }
}
