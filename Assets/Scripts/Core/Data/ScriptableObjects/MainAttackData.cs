using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    public class MainAttackData : AttackDataSO
    {
        [field: SerializeReference] public float Radius { get; private set; } = 1.5f;
        [field:SerializeReference] public float ForwardOffset {get; private set; } = 0.7f;
        [field:SerializeReference] public float AttackCooldown {get; private set; } = 0.8f;
    }
}