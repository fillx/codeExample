using System.Collections.Generic;
using System.Linq;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Features.CarSystems
{
    public class InstantiateCarSystem : ReactiveSystem<GameEntity>
    {
        private readonly GameContext _gameContext;
        private readonly IGroup<GameEntity> _drivewayGroup;

        public InstantiateCarSystem(GameContext contextsGame) : base(contextsGame)
        {
            _gameContext = contextsGame;
            _drivewayGroup = contextsGame.GetGroup(GameMatcher.Driveway);
            
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.InstantiateCarRequest.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return true;
        }
        
        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var requestEntity in entities)
            {
                var position = _drivewayGroup.GetEntities().First().spawnCarPoint.value.position;
                var carPrefab = Object.Instantiate(PrefabStorage.Instance.CarPrefab, position, quaternion.identity);
                var carEntity = _gameContext.CreateEntity();
                carEntity.AddRequestChangeBehaviourState(new CarSpawnedStateComponent());                
                carPrefab.Link(carEntity);
                requestEntity.Destroy();
            }
        }
    }
}