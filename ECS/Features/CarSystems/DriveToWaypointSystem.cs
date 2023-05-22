using System.Collections.Generic;
using System.Linq;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;
using UnityEngine;

namespace Game.Features.CarSystems
{
    public class DriveToWaypointSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _drivewayGroup;

        public DriveToWaypointSystem(GameContext contextsGame) : base(contextsGame)
        {
            _drivewayGroup = contextsGame.GetGroup(GameMatcher.Driveway);
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.CarSpawnedState.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isCar;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var carEntity in entities)
            {
                carEntity.pathfinderAgent.value.MoveToDestination(_drivewayGroup.GetEntities()
                    .First()
                    .waitingCarPoint.value,
                    () => carEntity.ReplaceRequestChangeBehaviourState(new CarWaitingStateComponent()));
            }
        }
    }
}