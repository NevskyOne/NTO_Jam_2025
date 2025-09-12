using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

public class Player : MonoBehaviour, IMovable, IAttack
{
    [Header("Компоненты")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Настройки")]
    [SerializeField] private float _groundCheckRadius = 0.1f;

    // Данные игрока
    private PlayerDataSO _playerData;
    private AttackDataSO _attackData;
    private MoveDataSO _moveData;

    // Состояния
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isDashing;
    private bool _isAttacking;
    private bool _isParrying;
    private int _jumpCount;
    private float _dashTimeLeft;
    private float _dashCooldownLeft;
    private float _attackCooldownLeft;
    private Vector2 _lastDirection = Vector2.right;

    // Логика
    private PlayerMovementLogic _movementLogic;
    private SomeAttackLogic _attackLogic;
    private SomeFoodLogic _foodLogic;
    private PlayerAnimationLogic _animationLogic;

    // Coroutine для парирования
    private Coroutine _currentParryCoroutine;

    [Inject]
    private void Construct(PlayerDataSO playerData, AttackDataSO attackData, MoveDataSO moveData)
    {
        _playerData = playerData;
        _attackData = attackData;
        _moveData = moveData;
    }

    private void Awake()
    {
        // Проверка наличия компонентов
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
        
        // Создание логики
        _movementLogic = new PlayerMovementLogic(_moveData, _rigidbody);
        _attackLogic = new SomeAttackLogic(_attackData);
        _foodLogic = new SomeFoodLogic();
        _animationLogic = new PlayerAnimationLogic();
    }

    private void Update()
    {
        UpdateTimers();
        CheckGrounded();
        
        // Отладочная информация о состоянии игрока
        if (Time.frameCount % 60 == 0) // Выводим раз в секунду
        {
            Debug.Log($"Player state: grounded={_isGrounded}, jumping={_isJumping}, dashing={_isDashing}, " +
                      $"attacking={_isAttacking}, parrying={_isParrying}, dashTimeLeft={_dashTimeLeft}, " +
                      $"dashCooldown={_dashCooldownLeft}, attackCooldown={_attackCooldownLeft}");
        }
    }

    private void UpdateTimers()
    {
        if (_dashTimeLeft > 0)
        {
            _dashTimeLeft -= Time.deltaTime;
            Debug.Log($"Dash time left: {_dashTimeLeft}");
            
            if (_dashTimeLeft <= 0)
            {
                _isDashing = false;
                Debug.Log("Dash ended");
            }
        }

        if (_dashCooldownLeft > 0)
        {
            _dashCooldownLeft -= Time.deltaTime;
            if (_dashCooldownLeft <= 0)
            {
                Debug.Log("Dash cooldown ended");
            }
        }

        if (_attackCooldownLeft > 0)
        {
            _attackCooldownLeft -= Time.deltaTime;
            if (_attackCooldownLeft <= 0)
            {
                _isAttacking = false;
                Debug.Log("Attack cooldown ended");
            }
        }
    }

    private void CheckGrounded()
    {
        if (_groundCheck == null) return;
        
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        
        if (_isGrounded)
        {
            _jumpCount = 0;
            if (_isJumping)
            {
                _isJumping = false;
                _animationLogic.PlayLandAnimation();
            }
        }
    }

    #region IMovable Implementation

    public void Move(Vector2 direction, float deltaTime)
    {
        if (_isAttacking || _isParrying) return;
        
        if (direction.x != 0)
        {
            _lastDirection = new Vector2(Mathf.Sign(direction.x), 0);
        }
        
        _movementLogic.Move(direction, deltaTime, _isGrounded);
        _animationLogic.PlayMoveAnimation(direction, _rigidbody.linearVelocity.magnitude);
    }

    public void Jump()
    {
        // Добавляем подробный отладочный вывод
        Debug.Log($"Jump attempt: isGrounded={_isGrounded}, jumpCount={_jumpCount}, maxJumpCount={_moveData.MaxJumpCount}, isAttacking={_isAttacking}, isParrying={_isParrying}, isDashing={_isDashing}");
        
        // Проверяем все условия, которые могут блокировать прыжок
        if (_isAttacking)
        {
            Debug.Log("Jump blocked: Player is attacking");
            return;
        }
        
        if (_isParrying)
        {
            Debug.Log("Jump blocked: Player is parrying");
            return;
        }
        
        if (_isDashing)
        {
            Debug.Log("Jump blocked: Player is dashing");
            return;
        }
        
        if (!_isGrounded && _jumpCount >= _moveData.MaxJumpCount)
        {
            Debug.Log($"Jump blocked: Not grounded and max jumps reached (jumpCount={_jumpCount}, maxJumpCount={_moveData.MaxJumpCount})");
            return;
        }
        
        _jumpCount++;
        _isJumping = true;
        _movementLogic.Jump();
        _animationLogic.PlayJumpAnimation();
        
        Debug.Log($"Jump executed: jumpCount={_jumpCount}");
    }

    public void Dash(Vector2 direction)
    {
        // Добавляем подробный отладочный вывод
        Debug.Log($"Dash attempt: isDashing={_isDashing}, dashCooldown={_dashCooldownLeft}, isAttacking={_isAttacking}, isParrying={_isParrying}, direction={direction}");
        
        if (_isDashing || _dashCooldownLeft > 0 || _isAttacking || _isParrying) 
        {
            Debug.Log("Dash failed: conditions not met");
            return;
        }
        
        // Если направление не указано, используем последнее направление движения
        if (direction == Vector2.zero)
        {
            direction = _lastDirection;
            Debug.Log($"Using last direction for dash: {direction}");
        }
        
        _isDashing = true;
        _dashTimeLeft = _moveData.DashDuration;
        _dashCooldownLeft = _moveData.DashCooldown;
        
        _lastDirection = direction.normalized;
        
        _movementLogic.Dash(_lastDirection);
        _animationLogic.PlayDashAnimation(_lastDirection);
        
        Debug.Log($"Dash executed in direction: {_lastDirection}, dashTimeLeft={_dashTimeLeft}, dashCooldown={_dashCooldownLeft}");
    }

    public bool IsGrounded()
    {
        return _isGrounded;
    }

    public Vector2 GetVelocity()
    {
        return _rigidbody.linearVelocity;
    }

    public void SetVelocity(Vector2 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }

    #endregion

    #region IAttack Implementation

    public void PerformAttack(Vector2 direction)
    {
        // Добавляем подробный отладочный вывод
        Debug.Log($"Attack attempt: isAttacking={_isAttacking}, isDashing={_isDashing}, isParrying={_isParrying}, attackCooldown={_attackCooldownLeft}");
        
        // Проверяем все условия, которые могут блокировать атаку
        if (_isAttacking)
        {
            Debug.Log("Attack blocked: Player is already attacking");
            return;
        }
        
        if (_isDashing)
        {
            Debug.Log("Attack blocked: Player is dashing");
            return;
        }
        
        if (_isParrying)
        {
            Debug.Log("Attack blocked: Player is parrying");
            return;
        }
        
        if (_attackCooldownLeft > 0)
        {
            Debug.Log($"Attack blocked: Attack on cooldown ({_attackCooldownLeft} seconds left)");
            return;
        }
        
        _isAttacking = true;
        _attackCooldownLeft = _attackData.AttackCooldown;
        
        if (direction != Vector2.zero)
        {
            _lastDirection = direction.normalized;
        }
        
        _attackLogic.PerformAttack(_lastDirection, transform.position);
        _animationLogic.PlayAttackAnimation(0);
        
        Debug.Log($"Attack executed: direction={_lastDirection}, cooldown={_attackCooldownLeft}");
    }

    public void PerformAirAttack(Vector2 direction)
    {
        if (_isGrounded || _isAttacking || _isDashing || _isParrying || _attackCooldownLeft > 0) return;
        
        _isAttacking = true;
        _attackCooldownLeft = _attackData.AttackCooldown;
        
        if (direction != Vector2.zero)
        {
            _lastDirection = direction.normalized;
        }
        
        _attackLogic.PerformAirAttack(_lastDirection, transform.position);
        _animationLogic.PlayAttackAnimation(1);
    }

    public void PerformParry()
    {
        // Добавляем отладочный вывод
        Debug.Log($"Parry attempt: isAttacking={_isAttacking}, isDashing={_isDashing}, isParrying={_isParrying}");
        
        if (_isAttacking || _isDashing || _isParrying) return;
        
        _isParrying = true;
        
        // Останавливаем предыдущую корутину, если она существует
        if (_currentParryCoroutine != null)
        {
            StopCoroutine(_currentParryCoroutine);
        }
        
        // Запускаем новую корутину и сохраняем ссылку на неё
        _currentParryCoroutine = StartCoroutine(ParryTimer());
        
        _attackLogic.PerformParry(transform.position);
        _animationLogic.PlayParryAnimation();
        
        Debug.Log("Parry executed");
    }
    
    // Корутина для сброса состояния парирования
    private System.Collections.IEnumerator ParryTimer()
    {
        yield return new WaitForSeconds(0.5f); // Время парирования
        _isParrying = false;
        _currentParryCoroutine = null;
        Debug.Log("Parry state reset");
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }

    public bool IsParrying()
    {
        return _isParrying;
    }

    public float GetDamage()
    {
        return _attackData.CalculateDamage();
    }

    public float GetAttackRadius()
    {
        return _attackData.AttackRadius;
    }

    #endregion

    // Метод для сброса всех состояний игрока
    public void ResetAllStates()
    {
        _isJumping = false;
        _isDashing = false;
        _isAttacking = false;
        _isParrying = false;
        _jumpCount = 0;
        _dashTimeLeft = 0;
        _dashCooldownLeft = 0;
        _attackCooldownLeft = 0;
        
        // Останавливаем все корутины
        if (_currentParryCoroutine != null)
        {
            StopCoroutine(_currentParryCoroutine);
            _currentParryCoroutine = null;
        }
        
        Debug.Log("All player states have been reset");
    }

    // Метод для использования еды/способностей
    public void UseFood(int foodType)
    {
        _foodLogic.UseFood(foodType, transform.position, _lastDirection);
        _animationLogic.PlayFoodAnimation(foodType);
    }

    // Метод для получения урона
    public void TakeDamage(int damage)
    {
        if (_isParrying) return; // Парирование блокирует урон
        
        _playerData.TakeDamage(damage);
        _animationLogic.PlayHitAnimation();
        
        if (_playerData.CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _animationLogic.PlayDeathAnimation();
        // Дополнительная логика смерти
        Debug.Log("Player died!");
    }

    // Для отладки
    private void OnDrawGizmos()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
