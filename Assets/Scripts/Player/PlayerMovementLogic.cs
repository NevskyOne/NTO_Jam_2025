using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

public class PlayerMovementLogic : IMovable
{
    private readonly MoveDataSO _moveData;
    private readonly Rigidbody2D _rigidbody;
    private bool _isGrounded;
    private int _jumpCount;
    private int _extraJumps;
    private float _dashCooldownTimer;
    private float _dashDurationTimer;
    
    public PlayerMovementLogic(MoveDataSO moveData, Rigidbody2D rigidbody)
    {
        _moveData = moveData;
        _rigidbody = rigidbody;
    }
    
    public void Move(Vector2 direction, float deltaTime)
    {
        if (_rigidbody == null) return;
        
        // Обновляем таймер dash кулдауна
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= deltaTime;
        }
        
        // Обновляем таймер dash длительности
        if (_dashDurationTimer > 0)
        {
            _dashDurationTimer -= deltaTime;
        }
        
        Vector2 velocity = _rigidbody.linearVelocity;
        float targetSpeedX = direction.x * _moveData.MoveSpeed;
        float accelerationMultiplier = _isGrounded ? 1f : _moveData.AirControlMultiplier;
        float acceleration = _moveData.Acceleration * accelerationMultiplier * deltaTime;
        float newVelocityX = Mathf.MoveTowards(velocity.x, targetSpeedX, acceleration);
        
        _rigidbody.linearVelocity = new Vector2(newVelocityX, velocity.y);
        _rigidbody.gravityScale = velocity.y < 0 ? _moveData.FallMultiplier : 1f;
    }
    
    public void Jump()
    {
        if (_rigidbody == null) return;
        int maxJumps = _moveData.MaxJumpCount + _extraJumps;
        if (!_isGrounded && _jumpCount >= maxJumps) return;
        _jumpCount++;
        _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _moveData.JumpForce);
    }
    
    public bool TryJump()
    {
        if (_rigidbody == null) return false;
        int maxJumps = _moveData.MaxJumpCount + _extraJumps;
        if (!_isGrounded && _jumpCount >= maxJumps) return false;
        
        _jumpCount++;
        _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _moveData.JumpForce);
        return true;
    }
    
    public void Dash(Vector2 direction)
    {
        if (_rigidbody == null) return;
        if (_dashCooldownTimer > 0) return;
        if (direction == Vector2.zero) direction = Vector2.right;
        
        // Используем только данные из MoveDataSO
        Vector2 dashVelocity = direction.normalized * _moveData.DashForce;
        
        // Сохраняем Y компонент скорости если падаем
        if (_rigidbody.linearVelocity.y < 0)
        {
            dashVelocity.y = _rigidbody.linearVelocity.y * 0.5f;
        }
        
        _rigidbody.linearVelocity = dashVelocity;
        _dashCooldownTimer = _moveData.DashCooldown;
        _dashDurationTimer = _moveData.DashDuration;
    }
    
    public void UpdateGrounded(bool isGrounded)
    {
        _isGrounded = isGrounded;
        if (_isGrounded)
        {
            _jumpCount = 0;
        }
    }
    
    public bool IsGrounded() => _isGrounded;
    public Vector2 GetVelocity() => _rigidbody != null ? _rigidbody.linearVelocity : Vector2.zero;
    public void SetVelocity(Vector2 velocity) { if (_rigidbody != null) _rigidbody.linearVelocity = velocity; }
    
    public void SetExtraJumps(int extra)
    {
        _extraJumps = Mathf.Max(0, extra);
    }
    
    public float GetDashDuration()
    {
        return _dashDurationTimer;
    }
}
