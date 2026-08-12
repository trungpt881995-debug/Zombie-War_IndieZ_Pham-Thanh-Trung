using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Controller;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Formation;
using ZombieWar.Features.Soldier.Model;
using ZombieWar.Features.Soldier.Movement;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Features.Soldier.Factories
{
    public sealed class SoldierGroupFactory :
        ISoldierGroupFactory
    {
        private readonly IEntityIdGenerator _ids;
        private readonly ISoldierFactory _soldierFactory;
        private readonly ISoldierMovementSolver _movementSolver;
        private readonly ISoldierGroupInputBuffer _inputBuffer;
        private readonly ITargetRangeProvider _targetRangeProvider;
        private readonly IEventBus _eventBus;

        public SoldierGroupFactory(IEntityIdGenerator ids,ISoldierFactory soldierFactory,ISoldierMovementSolver movementSolver,ISoldierGroupInputBuffer inputBuffer,ITargetRangeProvider targetRangeProvider,IEventBus eventBus)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));

            _soldierFactory = soldierFactory ?? throw new ArgumentNullException(nameof(soldierFactory));

            _movementSolver = movementSolver ?? throw new ArgumentNullException(nameof(movementSolver));

            _inputBuffer = inputBuffer ?? throw new ArgumentNullException(nameof(inputBuffer));

            _targetRangeProvider = targetRangeProvider ?? throw new ArgumentNullException(nameof(targetRangeProvider));

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public SoldierGroupController Create(ISoldierGroupView groupView, IReadOnlyList<ISoldierView> soldierViews, in SoldierSettings settings, IFormationProvider formationProvider)
        {
            if (groupView == null)
                throw new ArgumentNullException(nameof(groupView));

            if (soldierViews == null)
                throw new ArgumentNullException(nameof(soldierViews));

            if (soldierViews.Count != SoldierGroupController.MaxSoldiers)
            {
                throw new ArgumentException($"Exactly {SoldierGroupController.MaxSoldiers} Soldier views are required.", nameof(soldierViews));
            }

            if (formationProvider == null)
                throw new ArgumentNullException(nameof(formationProvider));

            // Group identity is allocated first so Shared Health and future
            // group-level systems can bind to a stable, primary EntityId.
            var model = new SoldierGroupModel(_ids.Next());

            var soldiers = new SoldierController[SoldierGroupController.MaxSoldiers];

            for (int i = 0;i < soldiers.Length;i++)
            {
                ISoldierView view = soldierViews[i];

                if (view == null)
                {
                    throw new ArgumentException($"Soldier view at index {i} is null.",nameof(soldierViews));
                }

                soldiers[i] =_soldierFactory.Create(i,view,in settings);
            }

            return new SoldierGroupController(model, groupView,soldiers, _movementSolver, _inputBuffer, formationProvider, _targetRangeProvider, _eventBus, in settings);
        }
    }
}
