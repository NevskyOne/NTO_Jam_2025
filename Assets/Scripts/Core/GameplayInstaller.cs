using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private PlayerConfig _playerConfig;

    public override void InstallBindings()
    {
        BindPlayer();
    }

    private void BindPlayer()
    {
        Container.Bind<PlayerConfig>().FromInstance(_playerConfig);
        var player = Container.InstantiatePrefabForComponent<Player>(_playerPrefab);
        Container.BindInterfacesAndSelfTo<Player>().FromInstance(player).AsSingle();
    }
}
