using System.Collections.Generic;
using System.Linq;
using Entitas;
using Game.Components.BehaviourStateCommon.Car;
using UnityEngine;

namespace Game.Features.CarSystems
{
    public class DiagnosedStateReactiveSystem : ReactiveSystem<GameEntity>
    {
        private readonly Contexts _contexts;
        private readonly IGroup<GameEntity> _diagnosticPointEntitiesGroup;
        private readonly IGroup<GameEntity> _brokenDetailEntitiesGroup;

        public DiagnosedStateReactiveSystem(Contexts contexts) : base(contexts.game)
        {
            _contexts = contexts;
            _diagnosticPointEntitiesGroup = contexts.game.GetGroup(GameMatcher.DiagnosticPoint);
            _brokenDetailEntitiesGroup = contexts.game.GetGroup(GameMatcher.AllOf(
                GameMatcher.Detail,
                GameMatcher.DisableState,
                GameMatcher.ParentCarHashcode));
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.DiagnosedState.Added());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isCar;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                var diagnosticPointEntity = _diagnosticPointEntitiesGroup.GetEntities().FirstOrDefault(e => e.parentCarHashcode.value == entity.hashCode.value);
                if (diagnosticPointEntity == null) Debug.LogError("diagnosticPointEntity is NULL");
                
                diagnosticPointEntity.triggerHide = true;
                
                _brokenDetailEntitiesGroup.GetEntities()
                    .Where(e => e.parentCarHashcode.value == entity.hashCode.value).ToList()
                    .ForEach(e => e.ReplaceRequestChangeBehaviourState(new BrokenStateComponent()));
            }
        }
    }
}