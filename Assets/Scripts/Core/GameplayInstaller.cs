using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private AttackDataSO attackDataSO;
    [SerializeField] private MoveDataSO moveDataSO;
    [SerializeField] private Player playerPrefab;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerDataSO>().FromInstance(playerDataSO.CreateRuntimeCopy()).AsSingle();
        Container.Bind<AttackDataSO>().FromInstance(attackDataSO.CreateRuntimeCopy()).AsSingle();
        Container.Bind<MoveDataSO>().FromInstance(moveDataSO.CreateRuntimeCopy()).AsSingle();
        Container.Bind<Player>().FromComponentInNewPrefab(playerPrefab).AsSingle();
    }
}
