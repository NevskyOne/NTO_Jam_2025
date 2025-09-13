using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
    public class PlayerDataSO : ScriptableObject
    {
        [Header("Здоровье")]
        [SerializeField] private int maxHealth = 100;
        
        [Header("Экономика")]
        [SerializeField] private int startMoney = 0;
        
        [Header("Репутация")]
        [SerializeField] private int startReputation = 0;
        [SerializeField] private int maxReputation = 100;
        
        // Переменные для рантайма
        private int _currentHealth;
        private int _currentMoney;
        private int _currentReputation;
        
        public int MaxHealth => maxHealth;
        public int CurrentHealth 
        { 
            get => _currentHealth; 
            set => _currentHealth = Mathf.Clamp(value, 0, maxHealth); 
        }
        
        public int CurrentMoney
        {
            get => _currentMoney;
            set => _currentMoney = Mathf.Max(0, value);
        }
        
        public int CurrentReputation
        {
            get => _currentReputation;
            set => _currentReputation = Mathf.Clamp(value, 0, maxReputation);
        }
        
        public PlayerDataSO CreateRuntimeCopy()
        {
            PlayerDataSO copy = Instantiate(this);
            copy.ResetRuntimeValues();
            return copy;
        }
        
        public void ResetRuntimeValues()
        {
            _currentHealth = maxHealth;
            _currentMoney = startMoney;
            _currentReputation = startReputation;
        }
        
        public void TakeDamage(int damage) => CurrentHealth -= damage;
        
        public void Heal(int amount) => CurrentHealth += amount;
        
        public void AddMoney(int amount) => CurrentMoney += amount;
        
        public bool SpendMoney(int amount)
        {
            if (CurrentMoney >= amount)
            {
                CurrentMoney -= amount;
                return true;
            }
            return false;
        }
    }
}
