using Core.Data.ScriptableObjects;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicEnemyAttackData", menuName = "Game Data/Enemy/Attack Data")]
public class BasicEnemyAttackDataSO : AttackDataSO
{
    [Header("Тип атаки")]
    [SerializeField] private AttackMode _mode = AttackMode.Melee;
    
    public AttackMode Mode => _mode;
    
    public enum AttackMode
    {
        Melee,
        Ranged
    }
}
