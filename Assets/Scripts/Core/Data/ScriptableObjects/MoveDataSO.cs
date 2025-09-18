using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MoveData", menuName = "Game Data/Move Data")]
    public class MoveDataSO : ScriptableObject
    {
        [Header("Базовое движение")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _maxMoveSpeed = 8f;
        [SerializeField] private float _acceleration = 50f;
        [SerializeField] private float _deceleration = 25f;
        
        [Header("Прыжок")]
        [SerializeField] private float _jumpForce = 12f;
        [SerializeField] private float _maxJumpHeight = 3f;
        [SerializeField] private float _jumpTimeThreshold = 0.25f;
        [SerializeField] private float _normalGravity = 9.8f;
        [SerializeField] private float _fallMultiplier = 2.5f;
        
        [Header("Деш")]
        [SerializeField] private float _dashForce = 20f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _dashCooldown = 1f;
        
        [Header("Дополнительные параметры")]
        [SerializeField] private float _airControlMultiplier = 0.7f;
        [SerializeField] private int _maxJumpCount = 1;
        
        public float MoveSpeed => _moveSpeed;
        public float MaxMoveSpeed => _maxMoveSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float JumpForce => _jumpForce;
        public float MaxJumpHeight => _maxJumpHeight;
        public float JumpTimeThreshold => _jumpTimeThreshold;
        public float NormalGravity => _normalGravity;
        public float FallMultiplier => _fallMultiplier;
        public float DashForce => _dashForce;
        public float DashDuration => _dashDuration;
        public float DashCooldown => _dashCooldown;
        public float AirControlMultiplier => _airControlMultiplier;
        public int MaxJumpCount => _maxJumpCount;
    }
}