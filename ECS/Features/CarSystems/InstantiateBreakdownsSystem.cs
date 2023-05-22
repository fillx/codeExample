using System;
using System.Collections.Generic;
using System.Linq;
using _4_Engine.Game.Components.BehaviourStateCommon;
using Core.Infrastructure.Config;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Features.CarSystems
{
    public class InstantiateBreakdownsSystem : ReactiveSystem<GameEntity>
    {
        private readonly IGroup<GameEntity> _mainStationGroup;
        private readonly GameContext _gameContext;
        private readonly IGroup<GameEntity> _unitStationGroup;

        public InstantiateBreakdownsSystem(GameContext contextsGame) : base(contextsGame)
        {
            _gameContext = contextsGame;
            _mainStationGroup = contextsGame.GetGroup(GameMatcher.AllOf(
                GameMatcher.ProductionMainStationId,
                GameMatcher.ProductionStateUnlocked));
            _unitStationGroup = contextsGame.GetGroup(GameMatcher.AllOf(
                GameMatcher.ProductionUnitStationId,
                GameMatcher.ProductionBoxState));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.Car.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isCar;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var carEntity in entities)
            {
                if (_mainStationGroup.count == 0) throw new NotImplementedException("No unit stations in the game!");
                var differentBreakdownsCount = UnityEngine.Random.Range(1, _mainStationGroup.count + 1);
                Debug.Log($" different breackdowns count {differentBreakdownsCount}");
                // var rnd = new System.Random();
                // var detailTypes = _mainStationGroup.GetEntities()
                //     .OrderBy(u => rnd.Next())
                //     .Select(e => e.detailType.value)
                //     .Distinct()
                //     .Take(differentBreakdownsCount);
                var ignoreList = new List<DetailType>();
                var detailTypes = _mainStationGroup.GetEntities()
                    .Distinct()
                    .Take(differentBreakdownsCount)
                    .Select(e => e.detailType.value);
                var breakdownsData = MainGameConfig.Instance.CarServiceSpotsCatalog.GetCatalogByTier(1);
                foreach (var type in detailTypes)
                {
                    var filteredBreakdownsSchemas = breakdownsData.breakdowns
                        .Where(x => detailTypes.Contains(x.Detail))
                        .Where(x => !ignoreList.Contains(x.Detail))
                        .ToArray();
                    var weights = filteredBreakdownsSchemas.Select(x => x.Weight).ToArray();
                    var rndIndex = RandomBreakdownType(weights);
                    var breakdownSchema =  filteredBreakdownsSchemas[rndIndex];
                    ignoreList.Add(breakdownSchema.Detail);

                    var openUnitsCount = _unitStationGroup.GetEntities().Count(x => x.detailType.value == type);
                    var max = Math.Min(breakdownSchema.MaxBreakdownsCount, openUnitsCount);
                    var brokenDetailCount = Random.Range(1, max);
                    var positions = breakdownSchema.GetRandomIndexes(brokenDetailCount);
                    foreach (var detailIndex in positions)
                    {
                        var detailEntity = _gameContext.GetEntitiesWithDetailIndex(detailIndex)
                            .First(x => x.parentCarHashcode.value == carEntity.hashCode.value);
                        detailEntity.AddBreakdownTypeComponent(type);
                        detailEntity.ReplaceRequestChangeBehaviourState(new DisableStateComponent());
                        detailEntity.AddDetailIcon(breakdownSchema.Icon);
                    }
                }
            }
        }
        private int RandomBreakdownType(int[] weights)
        {
            int randomWeight = UnityEngine.Random.Range(0, weights.Sum());

            for (int i = 0; i < weights.Length; i++)
            {
                randomWeight -= weights[i];

                if (randomWeight < 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}