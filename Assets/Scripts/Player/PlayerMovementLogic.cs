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
    
    public PlayerMovementLogic(MoveDataSO moveData, Rigidbody2D rigidbody)
    {
        _moveData = moveData;
        _rigidbody = rigidbody;
    }
    
    public void Move(Vector2 direction, float deltaTime)
    {
        if (_rigidbody == null) return;
        
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
    
    
    public void Dash(Vector2 direction)
    {
        if (_rigidbody == null) return;
        if (direction == Vector2.zero) direction = Vector2.right;
        float dashSpeed = Mathf.Min(_moveData.DashForce, 15f);
        _rigidbody.linearVelocity = direction.normalized * dashSpeed;
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
}
