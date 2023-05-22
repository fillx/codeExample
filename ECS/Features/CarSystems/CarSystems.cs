using System;

namespace Game.Features.CarSystems
{
    public class CarSystems : Feature
    {
        public CarSystems(Contexts contexts)
        {            
            Add(new SpawnCarSystem(contexts.game));
            Add(new InstantiateCarSystem(contexts.game));
            Add(new DriveToWaypointSystem(contexts.game));
            Add(new CheckFreeRepairStationsSystem(contexts.game));
            Add(new InstantiateBreakdownsSystem(contexts.game));
            Add(new DetailDisableStateReactiveSystem(contexts));
            Add(new CarRepairedReactSystem(contexts.game));
            Add(new RemoveCarFromRepairStationSystem(contexts.game));
            Add(new CarDriveAwaySystem(contexts.game));
            Add(new DiagnosedStateReactiveSystem(contexts));
            Add(new DetailBrokenStateReactiveSystem(contexts));
        }
    }
}