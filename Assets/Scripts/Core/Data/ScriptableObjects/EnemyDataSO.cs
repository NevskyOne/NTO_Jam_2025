using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Здоровье")]
        [SerializeField] private int _maxHealth = 100;
        
        [Header("Награда")]
        [SerializeField] private int _moneyReward = 10;
        
        public int MaxHealth => _maxHealth;
        public int MoneyReward => _moneyReward;
    }
}