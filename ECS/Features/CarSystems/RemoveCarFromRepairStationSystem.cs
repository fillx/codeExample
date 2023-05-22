using System.Collections.Generic;
using System.Linq;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;

namespace Game.Features.CarSystems
{
    public class RemoveCarFromRepairStationSystem : ReactiveSystem<GameEntity>
    {
        private readonly GameContext _gameContext;
        private readonly IGroup<GameEntity> _repairStationsGroup;

        public RemoveCarFromRepairStationSystem(GameContext contextsGame) : base(contextsGame)
        {
            _gameContext = contextsGame;
            _repairStationsGroup = contextsGame.GetGroup(GameMatcher.AllOf(
                GameMatcher.CarRepairStationTier,
                GameMatcher.CarRepairStationId,
                GameMatcher.CarInServiceHashcode));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.RepairedState.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isCar;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var car in entities)
            {
                var repairStation = _repairStationsGroup.GetEntities().First(e=>e.carInServiceHashcode.value == car.hashCode.value);
                repairStation.RemoveCarInServiceHashcode();
                car.ReplaceRequestChangeBehaviourState(new CarDriveAwayStateComponent());
            }
        }
    }
}