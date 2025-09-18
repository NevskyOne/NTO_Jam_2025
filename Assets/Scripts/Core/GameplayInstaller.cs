using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;


public class GameplayInstaller : MonoInstaller
{
    [Header("Data")] [SerializeField]  private PlayerDataSO _playerDataSo;
    [SerializeField] private MoveDataSO _moveDataSo;

    [Header("Components")]
    [SerializeField] private DialogueStartSystem _dialogueStartSystem;

    private ShopSystem _shopSystem;


    public override void InstallBindings()
    {
        _shopSystem = new ShopSystem();
        Container.BindInterfacesAndSelfTo<ShopSystem>().AsSingle().NonLazy();
        
        Container.Bind<PlayerDataSO>().FromInstance(_playerDataSo).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(_moveDataSo).AsSingle();
        Container.Bind<DialogueStartSystem>().FromInstance(_dialogueStartSystem).AsSingle();

        Container.Bind<Player>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<PlayerInput>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerMovementLogic>().AsSingle().NonLazy();

        Container.Bind<ShopUI>().FromComponentInHierarchy().AsSingle();
        Container.Bind<Camera>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PixelPerfectCamera>().FromComponentInHierarchy().AsSingle();
        Container.Bind<DragSystem>().AsSingle();

    }

}
