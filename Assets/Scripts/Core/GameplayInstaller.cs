using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using UnityEngine.InputSystem;

public class GameplayInstaller : MonoInstaller
{
    [Header("Data")]
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private MoveDataSO moveDataSO;
    [Header("Components")]
    [SerializeField] private new Camera camera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private DialogueStartSystem dialogueStartSystem;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerDataSO>().FromInstance(playerDataSO).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(moveDataSO).AsSingle();

        Container.Bind<Player>().FromInstance(playerPrefab).AsSingle().NonLazy();
        Container.Bind<PlayerInput>().FromInstance(playerInput).AsSingle().NonLazy();
        
        Container.Bind<Camera>().FromInstance(camera).AsSingle();
        Container.Bind<DialogueStartSystem>().FromInstance(dialogueStartSystem).AsSingle();
        Container.Bind<DragSystem>().AsSingle();
    }
}
