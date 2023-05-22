using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Enums;
using _Game.Scripts.Interfaces;
using _Game.Scripts.Systems;
using _Game.Scripts.Systems.Base;
using _Game.Scripts.Tools;
using _Game.Scripts.View.EntityBehaviour;
using _Game.Scripts.View.EntityBehaviour.Map;
using _Game.Scripts.View.Vehicle;
using Zenject;

namespace _Game.Scripts.Factories
{
    public class VehicleFactory : ITickableSystem
    {
        [Inject] private RoomsFactory _rooms;
        [Inject] private GameFlags _flags;

        private LevelSystem _levelSystem;
        
        private readonly CarView.Pool _pool;
        private readonly List<VehicleViewBase> _cars = new();
        
        public VehicleFactory(CarView.Pool pool, LevelSystem levelSystem)
        {
            _pool = pool;
            _levelSystem = levelSystem;
            _levelSystem.OnLoadedLevel += OnLoadedLevel;
            _levelSystem.OnDestroyLevel += OnDestroyLevel;
        }

        private void OnLoadedLevel()
        {
            var cars = _levelSystem.CurrentLevel.GetComponentsInChildren<VehicleViewBase>(true);
            foreach (var transport in cars)
            {
                transport.Init();
                if(!_cars.Contains(transport))_cars.Add(transport);
            }

            if (_flags.Has(GameFlag.TutorialFinished))
            {
                InitBackgroundCars();
            }
            else
            {
                _flags.OnFlagSet += OnFlagSet;
            }
        }

        private void OnFlagSet(GameFlag flag)
        {
            if(flag != GameFlag.TutorialFinished) return;
            InitBackgroundCars();
        }

        private void InitBackgroundCars()
        {
            for (int i = 0; i < _levelSystem.CurrentLevel.BackgroundCars; i++)
            {
                InvokeSystem.StartInvoke(SpawnBackgroundCar, i * 2f);
            }
        }

        private void SpawnBackgroundCar()
        {
            var car = SpawnVehicle();
            car.AddBehaviour(new BackgroundCarBehaviour());
        }
        
        public void Tick(float deltaTime)
        {
            if(_cars.Count == 0) return;
            for (var i = 0; i < _cars.Count; i++)
            {
                _cars[i].Tick(deltaTime);
            }
        }
        
        public CarView SpawnVehicle()
        {
            var car = _pool.Spawn();
            if(!_cars.Contains(car))_cars.Add(car);
            car.Activate();
            return car;
        }

        public CarView GetCar(RoomType roomType, int itemId)
        {
            var item = _rooms.GetRoomItem(roomType, itemId);
            if (item == null) return null;
            return item.GetComponentInChildren<CarView>();
        }

        public VehicleViewBase GetCar<T>() where T : EntityBehaviourBase
        {
            return _cars.FirstOrDefault(i => ((CarView)i).CurrentBehaviour is T);
        }

        public void RemoveVehicle(VehicleViewBase car)
        {
            car.OnDestroyLevel();
            _cars.Remove(car);
            if(car is CarView {SpawnedFromPool: true} view) _pool.Despawn(view);
        }
        
        private void OnDestroyLevel()
        {
            for (int i = 0; i < _cars.Count; )
            {
                RemoveVehicle(_cars[0]);
            }
            _cars.Clear();
        }
    }
}
