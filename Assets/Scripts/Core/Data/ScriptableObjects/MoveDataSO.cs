using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MoveData", menuName = "Game Data/Move Data")]
    public class MoveDataSO : ScriptableObject
    {
        [Header("Базовое движение")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float maxMoveSpeed = 8f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 25f;
        
        [Header("Прыжок")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float maxJumpHeight = 3f;
        [SerializeField] private float jumpTimeThreshold = 0.25f;
        [SerializeField] private float fallMultiplier = 2.5f;
        
        [Header("Деш")]
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        
        [Header("Дополнительные параметры")]
        [SerializeField] private float airControlMultiplier = 0.7f;
        [SerializeField] private int maxJumpCount = 1;
        
        public float MoveSpeed => moveSpeed;
        public float MaxMoveSpeed => maxMoveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float JumpForce => jumpForce;
        public float MaxJumpHeight => maxJumpHeight;
        public float JumpTimeThreshold => jumpTimeThreshold;
        public float FallMultiplier => fallMultiplier;
        public float DashForce => dashForce;
        public float DashDuration => dashDuration;
        public float DashCooldown => dashCooldown;
        public float AirControlMultiplier => airControlMultiplier;
        public int MaxJumpCount => maxJumpCount;
        
        public MoveDataSO CreateRuntimeCopy() => Instantiate(this);
    }
}
