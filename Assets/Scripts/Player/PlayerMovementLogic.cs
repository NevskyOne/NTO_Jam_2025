using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerMovementLogic : IMovable, ITickable
{
    private MoveDataSO _moveData;
    private Rigidbody2D _rigidbody;
    private bool _isGrounded;
    private int _jumpCount;
    private int _extraJumps;
    private float _dashCooldownTimer;
    private float _dashDurationTimer;
    private float _moveInputX; // cached horizontal input
    
    // Эффекты
    private float _speedMultiplier = 1f;
    private bool _isStunned = false;
    
    public Vector2 LastDirection { get; private set; } = Vector2.right;
    public bool IsGrounded() => _isGrounded;
    public float CurrentMoveSpeed => _moveData != null ? _moveData.MoveSpeed * _speedMultiplier : 0f;
    
    private Player _player;
    private PlayerInput _input;

    [Inject]
    private void Construct(Player player, PlayerInput input, MoveDataSO moveData, [InjectOptional] Rigidbody2D rb)
    {
        _player = player;
        _input = input;
        _moveData = moveData;
        _rigidbody = rb != null ? rb : player.GetComponent<Rigidbody2D>();

        _input.actions["Move"].performed += ctx => { _moveInputX = ReadMove(ctx); };
        _input.actions["Move"].canceled += _ => { _moveInputX = 0f; if (_player != null) _player.CameraTarget.localPosition = new Vector3(0,1,0); };
        _input.actions["Jump"].performed += _ => Jump();
        _input.actions["Shift"].performed += _ => Dash(LastDirection);
    }

    private float ReadMove(InputAction.CallbackContext ctx)
    {
        // Support both 1D and 2D bindings
        if (ctx.valueType == typeof(float)) return ctx.ReadValue<float>();
        if (ctx.valueType == typeof(Vector2)) return ctx.ReadValue<Vector2>().x;
        return 0f;
    }

    public void Tick()
    {
        Move(new Vector2(_moveInputX, 0));
    }
    
    public PlayerMovementLogic() {}

    public void Move(Vector2 direction)
    {
        // Проверки на null
        if (_moveData == null || _player == null || _player.CameraTarget == null || _rigidbody == null) return;
        
        // Если оглушен - не можем двигаться
        if (_isStunned)
        {
            _rigidbody.linearVelocity = new Vector2(0, _rigidbody.linearVelocity.y);
            return;
        }

        // Обновляем таймеры
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.fixedDeltaTime;
        if (_dashDurationTimer > 0) _dashDurationTimer -= Time.fixedDeltaTime;

        // Применяем эффекты скорости
        float currentSpeed = _moveData.MoveSpeed * _speedMultiplier;
        
        // Горизонтальное движение
        if (direction.x != 0)
        {
            LastDirection = new Vector2(direction.x, 0).normalized;
            _player.CameraTarget.localPosition = new Vector3(direction.x * 2, 1, 0);
        }

        // Dash logic
        if (_dashDurationTimer > 0)
        {
            _rigidbody.linearVelocity = new Vector2(LastDirection.x * _moveData.DashForce, _rigidbody.linearVelocity.y);
            return;
        }

        // Normal movement
        float targetVelocityX = direction.x * currentSpeed;
        float velocityChangeX = targetVelocityX - _rigidbody.linearVelocity.x;
        float force = velocityChangeX * _moveData.Acceleration;

        _rigidbody.AddForce(new Vector2(force, 0), ForceMode2D.Force);

        // Apply deceleration when not moving (используем Deceleration вместо Friction)
        if (Mathf.Abs(direction.x) < 0.1f)
        {
            float decelerationForce = -_rigidbody.linearVelocity.x * _moveData.Deceleration;
            _rigidbody.AddForce(new Vector2(decelerationForce, 0), ForceMode2D.Force);
        }
    }

    public void Jump()
    {
        if (_moveData == null || _rigidbody == null || _isStunned) return;

        // Используем MaxJumpCount вместо MaxJumps
        if (_isGrounded || _jumpCount < _moveData.MaxJumpCount + _extraJumps)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _moveData.JumpForce);
            if (!_isGrounded) _jumpCount++;
        }
    }

    public void Dash(Vector2 direction)
    {
        if (_moveData == null || _rigidbody == null || _isStunned) return;
        
        if (_dashCooldownTimer <= 0 && direction != Vector2.zero)
        {
            _dashCooldownTimer = _moveData.DashCooldown;
            _dashDurationTimer = _moveData.DashDuration;
            LastDirection = direction.normalized;
        }
    }

    public void UpdateGrounded(bool grounded)
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = grounded;
        
        if (_isGrounded && !wasGrounded)
        {
            _jumpCount = 0;
        }
    }

    public void AddExtraJump(int amount)
    {
        _extraJumps += amount;
    }

    public void RemoveExtraJump(int amount)
    {
        _extraJumps = Mathf.Max(0, _extraJumps - amount);
    }
    
    // Методы для эффектов
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }
    
    public void SetStunned(bool stunned)
    {
        _isStunned = stunned;
    }
    
    public bool IsStunned => _isStunned;
}
