using _4_Engine.Game.Features.CameraSystems;
using _4_Engine.Game.Features.Economic;
using _4_Engine.Game.Features.InputSystems;
using _4_Engine.Game.Features.Rewards;
using _4_Engine.Game.Features.Upgrades;
using _4_Engine.Game.Features.CarSystems;
using _4_Engine.Game.Features.Driveway;
using Entitas;

internal class GameCoreSystems : Systems
{
    public GameCoreSystems(Contexts contexts)
    {
        Add(new ExtensionFeature(contexts)); //<--- move state systems from auto systems
        
        //your systems here
        Add(new DrivewaySystems(contexts));
        Add(new InputSystems(contexts));
        Add(new UiInitSystem(contexts));
        Add(new CameraSystems(contexts));
        Add(new CarSystems(contexts));
        Add(new EconomicSystems(contexts));
        Add(new ProductionSystems(contexts));
        Add(new UpgradesSystems(contexts));
        Add(new RewardsSystems(contexts));    

        //auto
        Add(new GameEventSystems(contexts));
        Add(new GameCleanupSystems(contexts));
        Add(new InputCleanupSystems(contexts));
    }
}