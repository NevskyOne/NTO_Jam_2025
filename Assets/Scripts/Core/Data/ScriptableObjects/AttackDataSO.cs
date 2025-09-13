using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Game Data/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Характеристики атаки")]
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _attackRadius = 1f;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackDuration = 0.3f;
        [SerializeField] private float _knockbackForce = 5f;
        [SerializeField] private float _stunDuration = 0f;

        public float BaseDamage => _baseDamage;
        public float AttackRadius => _attackRadius;
        public float AttackCooldown => _attackCooldown;
        public float AttackDuration => _attackDuration;
        public float KnockbackForce => _knockbackForce;
        public float StunDuration => _stunDuration;
    }
}
