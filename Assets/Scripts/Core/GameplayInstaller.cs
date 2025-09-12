using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerConfig _playerConfig;

    public override void InstallBindings()
    {
        BindPlayer();
    }

    private void BindPlayer()
    {
        Container.Bind<PlayerConfig>().FromInstance(_playerConfig);
        Container.BindInterfacesAndSelfTo<Player>().FromInstance(_player).AsSingle();
        
        Container.Bind<Player>().FromInstance(_player).AsSingle();
    }
}
