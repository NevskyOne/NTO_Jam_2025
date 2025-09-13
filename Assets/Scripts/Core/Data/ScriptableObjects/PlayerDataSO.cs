using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
    public class PlayerDataSO : ScriptableObject
    {
        [Header("Здоровье")] 
        [SerializeField] private int _maxHealth = 100;

        [Header("Экономика")] 
        [SerializeField] private int _startMoney = 0;

        [Header("Репутация")] 
        [SerializeField] private int _startReputation = 0;
        [SerializeField] private int _maxReputation = 100;

        public int MaxHealth => _maxHealth;
        public int StartMoney => _startMoney;
        public int StartReputation => _startReputation;
        public int MaxReputation => _maxReputation;
    }
}
