using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Game Data/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Базовые характеристики атаки")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float attackRadius = 1f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackDuration = 0.3f;
        
        [Header("Дополнительные эффекты")]
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float stunDuration = 0f;
        
        public float BaseDamage => baseDamage;
        public float DamageMultiplier => damageMultiplier;
        public float AttackRadius => attackRadius;
        public float AttackCooldown => attackCooldown;
        public float AttackDuration => attackDuration;
        public float KnockbackForce => knockbackForce;
        public float StunDuration => stunDuration;
        
        public float CalculateDamage() => baseDamage * damageMultiplier;
        
        public AttackDataSO CreateRuntimeCopy() => Instantiate(this);
    }
}
