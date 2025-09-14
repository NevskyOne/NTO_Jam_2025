using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Game Data/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Базовые характеристики атаки")]
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _attackRadius = 1f;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackDuration = 0.3f;
        [SerializeField] private float _knockbackForce = 5f;
        [SerializeField] private float _stunDuration = 0f;

        [Header("Основная атака (ЛКМ)")]
        [SerializeField] private float _mainAttackDamage = 15f;
        [SerializeField] private float _mainAttackRadiusMultiplier = 1.3f;
        [SerializeField] private float _mainAttackForwardOffset = 0.7f;
        [SerializeField] private float _mainAttackCooldown = 0.8f;

        [Header("Атака вниз (S в воздухе)")]
        [SerializeField] private float _downAttackDamage = 20f;
        [SerializeField] private float _downAttackRadiusMultiplier = 1.5f;
        [SerializeField] private float _downAttackDownOffset = 0.6f;
        [SerializeField] private float _downAttackCooldown = 1.0f;

        [Header("Парирование (ПКМ)")]
        [SerializeField] private float _parryRadiusMultiplier = 1.4f;
        [SerializeField] private float _parryCooldown = 0.6f;
        [SerializeField] private float _parryDuration = 0.3f;

        public float BaseDamage => _baseDamage;
        public float AttackRadius => _attackRadius;
        public float AttackCooldown => _attackCooldown;
        public float AttackDuration => _attackDuration;
        public float KnockbackForce => _knockbackForce;
        public float StunDuration => _stunDuration;

        // Основная атака
        public float MainAttackDamage => _mainAttackDamage;
        public float MainAttackRadiusMultiplier => _mainAttackRadiusMultiplier;
        public float MainAttackForwardOffset => _mainAttackForwardOffset;
        public float MainAttackCooldown => _mainAttackCooldown;

        // Атака вниз
        public float DownAttackDamage => _downAttackDamage;
        public float DownAttackRadiusMultiplier => _downAttackRadiusMultiplier;
        public float DownAttackDownOffset => _downAttackDownOffset;
        public float DownAttackCooldown => _downAttackCooldown;

        // Парирование
        public float ParryRadiusMultiplier => _parryRadiusMultiplier;
        public float ParryCooldown => _parryCooldown;
        public float ParryDuration => _parryDuration;
    }
}
