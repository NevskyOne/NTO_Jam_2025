using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using UnityEngine.InputSystem;

public class GameplayInstaller : MonoInstaller
{
    [Header("Data")]
    [SerializeField] private PlayerDataSO _playerDataSO;
    [SerializeField] private AttackDataSO _attackDataSO;
    [SerializeField] private MoveDataSO _moveDataSO;
    [Header("Input")] 
    [SerializeField] private InputActionAsset _inputActions;
    [Header("MonoBehaviors")]
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private DialogueStartSystem _dialogueStartSystem;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerDataSO>().FromInstance(_playerDataSO).AsSingle();
        Container.Bind<AttackDataSO>().FromInstance(_attackDataSO).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(_moveDataSO).AsSingle();
        Container.Bind<Player>().FromComponentInNewPrefab(_playerPrefab).AsSingle();

        Container.Bind<InputActionAsset>().FromInstance(_inputActions).AsSingle();

        Container.Bind<Camera>().FromInstance(_camera).AsSingle();
        Container.Bind<PlayerInput>().FromInstance(_playerInput).AsSingle();
        Container.Bind<DialogueStartSystem>().FromInstance(_dialogueStartSystem).AsSingle();
        
    }
}
