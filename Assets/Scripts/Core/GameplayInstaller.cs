using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameplayInstaller : MonoInstaller
{
    [Header("Data")] [SerializeField] private PlayerDataSO _playerDataSo;
    [SerializeField] private MoveDataSO _moveDataSo;

    [Header("Components")] [SerializeField]
    private new Camera _camera;

    [SerializeField] private DialogueStartSystem _dialogueStartSystem;

    public override void InstallBindings()
    {
        Container.Bind<PlayerDataSO>().FromInstance(_playerDataSo).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(_moveDataSo).AsSingle();

        Container.Bind<Player>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<PlayerInput>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerMovementLogic>().AsSingle().NonLazy();

        Container.Bind<Camera>().FromInstance(_camera).AsSingle();
        Container.Bind<DialogueStartSystem>().FromInstance(_dialogueStartSystem).AsSingle();
        Container.Bind<DragSystem>().AsSingle();
    }
}
