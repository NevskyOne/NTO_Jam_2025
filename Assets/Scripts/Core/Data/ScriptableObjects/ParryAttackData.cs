using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ParryAttackData", menuName = "Game Data/Attacks/Parry Attack Data")]
    public class ParryAttackData : AttackDataSO
    {
         [field:SerializeReference] public float Duration {get; private set; } = 0.3f;
         
    } 
}