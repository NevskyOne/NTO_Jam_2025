using UnityEngine;


[CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Health")] 
    [SerializeField] private int _maxHealth = 3;
    [Header("Economy")] 
    [SerializeField] private int _startMoney;
    [Header("reputation")] 
    [SerializeField] private int _startReputation;

    public int MaxHealth => _maxHealth;
    public int StartMoney => _startMoney;
    public int StartReputation => _startReputation;

}

