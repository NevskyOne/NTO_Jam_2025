using UnityEngine;
using Core.Data.ScriptableObjects;

public class PlayerMovementLogic
{
    private readonly MoveDataSO _moveData;
    private readonly Rigidbody2D _rigidbody;
    
    public PlayerMovementLogic(MoveDataSO moveData, Rigidbody2D rigidbody)
    {
        _moveData = moveData ?? ScriptableObject.CreateInstance<MoveDataSO>();
        _rigidbody = rigidbody;
    }
    
    public void Move(Vector2 direction, float deltaTime, bool isGrounded)
    {
        if (_rigidbody == null) return;
        
        Vector2 velocity = _rigidbody.linearVelocity;
        float targetSpeedX = direction.x * _moveData.MoveSpeed;
        float accelerationMultiplier = isGrounded ? 1f : _moveData.AirControlMultiplier;
        float acceleration = _moveData.Acceleration * accelerationMultiplier * deltaTime;
        float newVelocityX = Mathf.MoveTowards(velocity.x, targetSpeedX, acceleration);
        
        _rigidbody.linearVelocity = new Vector2(newVelocityX, velocity.y);
        _rigidbody.gravityScale = velocity.y < 0 ? _moveData.FallMultiplier : 1f;
    }
    
    public void Jump()
    {
        if (_rigidbody == null) return;
        _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _moveData.JumpForce);
    }
    
    public void Dash(Vector2 direction)
    {
        if (_rigidbody == null) return;
        
        if (direction == Vector2.zero) direction = Vector2.right;
        float dashSpeed = Mathf.Min(_moveData.DashForce, 15f);
        _rigidbody.linearVelocity = direction.normalized * dashSpeed;
    }
}
