using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Game Data/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Base")]
        [field: SerializeField] public string InputBinding { get; private set; } = "";
        [field: SerializeField] public int BaseDamage { get; private set; } = 1;
        [field: SerializeField] public float Radius { get; private set; } = 1f;
        [field: SerializeField] public float AttackCooldown { get; private set; } = 1f;
    }
}
