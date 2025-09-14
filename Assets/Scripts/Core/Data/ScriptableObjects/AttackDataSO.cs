using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Game Data/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Base")]
        [field:SerializeReference] public string InputBinding {get; private set; }
        [field:SerializeReference] public int BaseDamage {get; private set; }
    }
}
