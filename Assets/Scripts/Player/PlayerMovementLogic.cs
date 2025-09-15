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
    
    public Vector2 LastDirection { get; private set; } = Vector2.right;
    public bool IsGrounded() => _isGrounded;
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
        if (_rigidbody == null) return;
        if (_player != null && _player.State is Player.PlayerState.Attacking or Player.PlayerState.Parrying or Player.PlayerState.Dashing)
        {
            return;
        }

        LastDirection = direction;
        
        if( direction.x > 0) _player.CameraTarget.localPosition = new Vector3(0.5f,1,0);
        else if( direction.x < 0) _player.CameraTarget.localPosition = new Vector3(-0.5f,1,0);
        
        // Обновляем таймер dash кулдауна
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
        
        // Обновляем таймер dash длительности
        if (_dashDurationTimer > 0)
        {
            _dashDurationTimer -= Time.deltaTime;
        }
        
        Vector2 velocity = _rigidbody.linearVelocity;
        float targetSpeedX = direction.x * _moveData.MoveSpeed;
        float accelerationMultiplier = _isGrounded ? 1f : _moveData.AirControlMultiplier;
        float acceleration = _moveData.Acceleration * accelerationMultiplier * Time.deltaTime;
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
        if (_dashCooldownTimer > 0) return;
        if (direction == Vector2.zero) direction = Vector2.right;
        
        Vector2 dashVelocity = direction.normalized * _moveData.DashForce;
        
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
    
    public void SetExtraJumps(int extra)
    {
        _extraJumps = Mathf.Max(0, extra);
    }
}
