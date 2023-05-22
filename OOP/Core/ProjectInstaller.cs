using _Game.Scripts.Balance;
using _Game.Scripts.ScriptableObjects;
using _Game.Scripts.Systems.Analytics;
using _Game.Scripts.Systems.Base;
using _Game.Scripts.Tools;
using UnityEngine;
using Zenject;

namespace _Game.Scripts.Core
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private ProjectSettings _projectSettings;
        [SerializeField] private Prefabs _prefabs;
        [SerializeField] private GameResources _resources;
        [SerializeField] private GameBalanceConfigs _balances;
        [SerializeField] private Localization _localization;

        public override void Start()
        {
#if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;
#else
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
            base.Start();
        }

        public override void InstallBindings()
        {
            BindScriptableObjects();
            BindSystems();
            
            PrettyPrint.Init(Container.Resolve<Localization>());
        }

        private void BindScriptableObjects()
        {
            Container.BindInstance(_projectSettings).AsSingle().NonLazy();
            Container.BindInstance(_prefabs).AsSingle().NonLazy();
            Container.BindInstance(_resources).AsSingle().NonLazy();
            Container.BindInstance(_balances).AsSingle().NonLazy();
            Container.BindInstance(_localization).AsSingle().NonLazy();
            
            _prefabs.Initialize();
            _localization.Initialize();

            InitExecutionOrder();
        }

        private void BindSystems()
        {
            Container.Bind<GameFlags>().AsSingle().NonLazy();
            Container.Bind<AnalyticsSystem>().AsSingle().NonLazy();
        }

        private void InitExecutionOrder()
        {
        }
    }
}