using System.Collections.Generic;
using Entitas;

namespace Game.Features.CarSystems
{
    public class CarDriveAwaySystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _exitPointsGroup;

        public CarDriveAwaySystem(GameContext contextsGame) : base(contextsGame)
        {
            _exitPointsGroup = contextsGame.GetGroup(GameMatcher.ExitCarPoint);
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.CarDriveAwayState.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isCar;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var car in entities)
            {
                var exitPointTransform = _exitPointsGroup.GetEntities()[0].exitCarPoint.value;
                car.pathfinderAgent.value.MoveToDestination(exitPointTransform, () => car.Destroy());
            }
        }
    }
}