using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemyDataSO", menuName = "Data/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [field: SerializeField] public int MaxHealth { get; private set; } = 10;
        [field: SerializeField] public float MoveSpeed { get; private set; } = 3f;
        [field: SerializeField] public int Damage { get; private set; } = 1;
    }
}
