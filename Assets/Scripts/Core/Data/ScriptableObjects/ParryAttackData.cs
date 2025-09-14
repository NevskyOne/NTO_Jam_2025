using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    public class ParryAttackData : AttackDataSO
    {
        [field: SerializeReference] public float Radius { get; private set; } = 1.4f;
        [field:SerializeReference] public float Duration {get; private set; } = 0.3f;
        [field:SerializeReference] public float AttackCooldown {get; private set; } = 0.6f;
    }
}