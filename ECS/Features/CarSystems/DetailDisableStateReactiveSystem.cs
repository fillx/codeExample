using System.Collections.Generic;
using Entitas;

namespace Game.Features.CarSystems
{
    public class DetailDisableStateReactiveSystem : ReactiveSystem<GameEntity>
    {
        private readonly Contexts _contexts;

        public DetailDisableStateReactiveSystem(Contexts contexts) : base(contexts.game)
        {
            _contexts = contexts;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.DisableState.Added());
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
        }
    }
}