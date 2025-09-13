using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using UnityEngine.InputSystem;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private AttackDataSO attackDataSO;
    [SerializeField] private MoveDataSO moveDataSO;
    [SerializeField] private Player playerPrefab;
    [Header("Input")] 
    [SerializeField] private InputActionAsset inputActions;
     
    public override void InstallBindings()
    {
        Container.Bind<PlayerDataSO>().FromInstance(playerDataSO).AsSingle();
        Container.Bind<AttackDataSO>().FromInstance(attackDataSO).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(moveDataSO).AsSingle();
        Container.Bind<Player>().FromComponentInNewPrefab(playerPrefab).AsSingle();
        if (inputActions != null)
        {
            Container.Bind<InputActionAsset>().FromInstance(inputActions).AsSingle();
        }
    }
}
