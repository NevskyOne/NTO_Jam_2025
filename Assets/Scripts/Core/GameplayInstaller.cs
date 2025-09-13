using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using UnityEngine.InputSystem;

public class GameplayInstaller : MonoInstaller
{
    [Header("Data")]
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private AttackDataSO attackDataSO;
    [SerializeField] private MoveDataSO moveDataSO;
    
    [Header("MonoBehaviors")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private DialogueStartSystem _dialogueStartSystem;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerDataSO>().FromInstance(playerDataSO.CreateRuntimeCopy()).AsSingle();
        Container.Bind<AttackDataSO>().FromInstance(attackDataSO.CreateRuntimeCopy()).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(moveDataSO.CreateRuntimeCopy()).AsSingle();
        Container.Bind<Player>().FromComponentInNewPrefab(playerPrefab).AsSingle();

        Container.Bind<Camera>().FromInstance(_camera).AsSingle();
        Container.Bind<PlayerInput>().FromInstance(_playerInput).AsSingle();
        Container.Bind<DialogueStartSystem>().FromInstance(_dialogueStartSystem).AsSingle();
        
    }
}
