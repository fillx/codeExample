using System;
using System.Collections.Generic;
using System.Linq;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;
using UnityEngine;

namespace Game.Features.CarSystems
{
    public class CarRepairedReactSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _carGroup;
        private readonly GameContext _gameContext;
        

        public CarRepairedReactSystem(GameContext contextsGame) : base(contextsGame)
        {
            _gameContext = contextsGame;
            _carGroup = contextsGame.GetGroup(GameMatcher.AllOf(GameMatcher.Car, GameMatcher.DiagnosedState));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.RepairedState.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isDetail;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.triggerHide = true;
            }
            
            foreach (var carEntity in _carGroup.GetEntities())
            {
                var details = _gameContext.GetEntitiesWithParentCarHashcode(carEntity.hashCode.value)
                    .Where(e => e.isDetail && e.hasDetailType);
                var all = details.All(d => d.isRepairedState);
                Debug.Log($"Car {carEntity.hashCode.value} repaired: {all}");
                if(all) carEntity.ReplaceRequestChangeBehaviourState(new RepairedStateComponent());
            }
        }
    }
}