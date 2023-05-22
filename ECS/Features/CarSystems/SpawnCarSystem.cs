using System.Collections.Generic;
using Entitas;

namespace Game.Features.CarSystems
{
    public class SpawnCarSystem : ReactiveSystem<GameEntity>
    {
        private readonly GameContext _gameContext;

        public SpawnCarSystem(GameContext contextsGame) : base(contextsGame)
        {
            _gameContext = contextsGame;
        }


        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.CarInDetectionArea.Removed());
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.isCarInDetectionArea == false;
        }

        protected override void Execute(List<GameEntity> entities)
        {
            _gameContext.CreateEntity().isInstantiateCarRequest = true;
        }
    }
}