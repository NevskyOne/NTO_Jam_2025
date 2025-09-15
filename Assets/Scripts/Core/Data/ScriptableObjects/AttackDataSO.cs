using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Game Data/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Base")]
        [field: SerializeField] public string InputBinding { get; private set; } = "";
        [field: SerializeField] public int BaseDamage { get; private set; } = 1;
    }
}
