using Entitas;
using UnityEngine;

public class GameCore : MonoBehaviour
{
    private Contexts _contexts;
    private Systems _gameCoreSystems;
    [SerializeField] private MainGameConfig mainGameConfig;
    [SerializeField] private PrefabStorage prefabStorage;

    private void Awake()
    {
        MainGameConfig.Instance = mainGameConfig;
        PrefabStorage.Instance = prefabStorage;
        _contexts = Contexts.sharedInstance;
        _gameCoreSystems = new GameCoreSystems(_contexts);
    }

    private void Start()
    {
        _gameCoreSystems.Initialize();
    }

    private void Update()
    {
        _gameCoreSystems.Execute();
        _gameCoreSystems.Cleanup();
    }

    private void OnDestroy()
    {
        _gameCoreSystems.TearDown();
        _gameCoreSystems.DeactivateReactiveSystems();
        _gameCoreSystems.ClearReactiveSystems();
    }
}
   