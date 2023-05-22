using System.Collections.Generic;
using System.Linq;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;

namespace Game.Features.CarSystems
{
    public class CheckFreeRepairStationsSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _carGroup;
        private readonly IGroup<GameEntity> _repairStationsGroup;

        public CheckFreeRepairStationsSystem(GameContext contextsGame) : base(contextsGame)
        {
            _carGroup = contextsGame.GetGroup(GameMatcher.CarWaitingState);
            _repairStationsGroup = contextsGame.GetGroup(
                GameMatcher.AllOf(GameMatcher.CarRepairStationId, GameMatcher.ProductionStateUnlocked)
                    .NoneOf(GameMatcher.CarInServiceHashcode));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.CarWaitingState.Added(),
                GameMatcher.CarInServiceHashcode.Removed(),
                GameMatcher.ProductionStateUnlocked.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return true;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var car in _carGroup)
            {
                var freeRepairStation = _repairStationsGroup.GetEntities().FirstOrDefault(x => x.isOccupied == false);
                if (freeRepairStation == null) continue;
                freeRepairStation.isOccupied = true;
                car.ReplaceRequestChangeBehaviourState(new CarDriveToWaypointStateComponent());
                car.pathfinderAgent.value.MoveToDestination(freeRepairStation.navigationPoint.value, () =>
                {
                    car.ReplaceRequestChangeBehaviourState(new CarOnRepairPointStateComponent());
                    freeRepairStation.ReplaceCarInServiceHashcode(car.hashCode.value);
                    freeRepairStation.isOccupied = false;
                });
            }
        }
    }
}