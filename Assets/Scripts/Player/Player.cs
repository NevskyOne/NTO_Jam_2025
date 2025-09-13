using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IHittable, IHealable
{
    [Header("Компоненты")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GroundChecker _groundChecker;

    // Данные игрока
    private PlayerDataSO _playerData;
    private AttackDataSO _attackData;
    private MoveDataSO _moveData;

    // Input System (через DI)
    private InputActionAsset _inputActions;
    private InputActionMap _map;
    private InputAction _move;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _leftMouseAction;
    private InputAction _rightMouseAction;
    private InputAction _qAction;
    private InputAction _eAction;
    private InputAction _downAttackAction; // опционально

    // Состояния
    private enum PlayerState { Idle, Moving, Jumping, Dashing, Attacking, Parrying }
    private PlayerState _state = PlayerState.Idle;
    private float _dashTimeLeft;
    private float _dashCooldownLeft;
    private float _attackCooldownLeft;
    private Vector2 _lastDirection = Vector2.right;
    private Vector2 _cachedMove; // кэш ввода движения

    // Логика движения и атак
    private IMovable _movement;
    private IAttack _mainAttack;
    private IAttack _downAttack;
    private IAttack _parryAttack;

    // Наборы атак
    private List<IAttack> _mainAttackSet = new();
    private List<IAttack> _abilitiesSet = new();

    // Runtime-значения PlayerData
    private int _currentHealth;
    private int _currentMoney;
    private int _currentReputation;

    // Короутина парирования
    private Coroutine _currentParryCoroutine;

    [SerializeField] private Transform _dialogueBubblePos;
    public Transform DialogueBubblePos => _dialogueBubblePos;

    [Inject]
    private void Construct(PlayerDataSO playerData, AttackDataSO attackData, MoveDataSO moveData, InputActionAsset inputActions)
    {
        _playerData = playerData;
        _attackData = attackData;
        _moveData = moveData;
        _inputActions = inputActions;
    }

    private void Awake()
    {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<Collider2D>();

        _movement = new PlayerMovementLogic(_moveData, _rigidbody);
        _mainAttack = new MainAttackLogic(_attackData, transform);
        _downAttack = new DownAttackLogic(_attackData, transform);
        _parryAttack = new ParryAttackLogic(_attackData, transform);

        _currentHealth = _playerData != null ? _playerData.MaxHealth : 100;
        _currentMoney = _playerData != null ? _playerData.StartMoney : 0;
        _currentReputation = _playerData != null ? _playerData.StartReputation : 0;

        // Инициализируем набор основной атаки базовой атакой
        _mainAttackSet.Clear();
        _mainAttackSet.Add(_mainAttack);
    }

    private void OnEnable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged += _movement.UpdateGrounded;

        // Настройка и подписка на Input Actions
        if (_inputActions != null)
        {
            _map = _inputActions.FindActionMap("Player", throwIfNotFound: true);
            _move = _map.FindAction("Move", throwIfNotFound: true);
            _jumpAction = _map.FindAction("Jump", throwIfNotFound: true);
            _dashAction = _map.FindAction("Shift", throwIfNotFound: true);
            _leftMouseAction = _map.FindAction("LeftMouse", throwIfNotFound: true);
            _rightMouseAction = _map.FindAction("RightMouse", throwIfNotFound: true);
            _qAction = _map.FindAction("Q", throwIfNotFound: true);
            _eAction = _map.FindAction("E", throwIfNotFound: true);
            _downAttackAction = _map.FindAction("DownAttack", throwIfNotFound: false);

            // Move
            _move.performed += OnMovePerformed;
            _move.canceled += OnMoveCanceled;
            // Jump/Dash
            _jumpAction.performed += OnJumpPerformed;
            _dashAction.performed += OnDashPerformed;
            // Combat/Abilities
            _leftMouseAction.performed += OnLeftMousePerformed;
            _rightMouseAction.performed += OnRightMousePerformed;
            _qAction.performed += OnQPerformed;
            _eAction.performed += OnEPerformed;
            if (_downAttackAction != null) _downAttackAction.performed += OnDownAttackPerformed;

            _map.Enable();
        }
        else
        {
            Debug.LogError("Player: InputActionAsset не привязан в GameplayInstaller");
        }
    }

    private void OnDisable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged -= _movement.UpdateGrounded;

        if (_map != null)
        {
            if (_move != null) { _move.performed -= OnMovePerformed; _move.canceled -= OnMoveCanceled; }
            if (_jumpAction != null) _jumpAction.performed -= OnJumpPerformed;
            if (_dashAction != null) _dashAction.performed -= OnDashPerformed;
            if (_leftMouseAction != null) _leftMouseAction.performed -= OnLeftMousePerformed;
            if (_rightMouseAction != null) _rightMouseAction.performed -= OnRightMousePerformed;
            if (_qAction != null) _qAction.performed -= OnQPerformed;
            if (_eAction != null) _eAction.performed -= OnEPerformed;
            if (_downAttackAction != null) _downAttackAction.performed -= OnDownAttackPerformed;
            _map.Disable();
        }
    }

    private void Update()
    {
        UpdateTimers();

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Player: state={_state}, grounded={_movement.IsGrounded()}, dashTime={_dashTimeLeft}, dashCd={_dashCooldownLeft}, atkCd={_attackCooldownLeft}");
        }

        // Движение — каждый кадр
        Move(_cachedMove, Time.deltaTime);
    }

    // Input callbacks
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        // Поддержка 2DVector и 1D Axis
        try { _cachedMove = ctx.ReadValue<Vector2>(); }
        catch { float x = 0f; try { x = ctx.ReadValue<float>(); } catch { x = 0f; } _cachedMove = new Vector2(x, 0f); }
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _cachedMove = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx) { Jump(); }
    private void OnDashPerformed(InputAction.CallbackContext ctx) { Dash(_cachedMove.normalized); }
    private void OnLeftMousePerformed(InputAction.CallbackContext ctx)
    {
        // Фолбэк для удара вниз без отдельного действия: в воздухе + удержание "вниз"
        if (!_movement.IsGrounded() && _cachedMove.y < -0.5f)
        {
            PerformAirAttack(_cachedMove);
            return;
        }
        PerformAttack(_cachedMove);
    }
    private void OnRightMousePerformed(InputAction.CallbackContext ctx) { PerformParry(); }
    private void OnQPerformed(InputAction.CallbackContext ctx) { UseFood(0); }
    private void OnEPerformed(InputAction.CallbackContext ctx) { UseFood(1); }
    private void OnDownAttackPerformed(InputAction.CallbackContext ctx) { if (!_movement.IsGrounded()) PerformAirAttack(_cachedMove); }

    private void UpdateTimers()
    {
        if (_dashTimeLeft > 0)
        {
            _dashTimeLeft -= Time.deltaTime;
            if (_dashTimeLeft <= 0 && _state == PlayerState.Dashing)
            {
                _state = PlayerState.Idle;
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
            if (_attackCooldownLeft <= 0 && _state == PlayerState.Attacking)
            {
                _state = PlayerState.Idle;
                Debug.Log("Attack cooldown ended");
            }
        }
    }

    private void OnGroundChanged(bool grounded) { /* больше не используется; землю обрабатывает IMovable */ }

    // Входные методы движения/боя
    public void Move(Vector2 direction, float deltaTime)
    {
        if (_state == PlayerState.Attacking || _state == PlayerState.Parrying) return;
        if (direction.x != 0) _lastDirection = new Vector2(Mathf.Sign(direction.x), 0);
        _movement.Move(direction, deltaTime);
        _state = Mathf.Abs(direction.x) > 0.01f ? PlayerState.Moving : PlayerState.Idle;
    }

    public void Jump()
    {
        Debug.Log($"Jump attempt: state={_state}, grounded={_movement.IsGrounded()}");
        if (_state is PlayerState.Attacking or PlayerState.Parrying or PlayerState.Dashing) return;

        _movement.Jump();
        _state = PlayerState.Jumping;
        Debug.Log($"Jump executed");
    }

    public void Dash(Vector2 direction)
    {
        Debug.Log($"Dash attempt: state={_state}, dashCd={_dashCooldownLeft}, dir={direction}");
        if (_state is PlayerState.Dashing or PlayerState.Attacking or PlayerState.Parrying) return;
        if (_dashCooldownLeft > 0) return;

        if (direction == Vector2.zero)
        {
            direction = _lastDirection;
            Debug.Log($"Using last direction for dash: {direction}");
        }

        _state = PlayerState.Dashing;
        _dashTimeLeft = _moveData.DashDuration;
        _dashCooldownLeft = _moveData.DashCooldown;
        _lastDirection = direction.normalized;
        _movement.Dash(_lastDirection);

        Debug.Log($"Dash executed dir={_lastDirection}, dashTime={_dashTimeLeft}, cd={_dashCooldownLeft}");
    }

    public bool IsGrounded() => _movement.IsGrounded();
    public Vector2 GetVelocity() => _movement.GetVelocity();
    public void SetVelocity(Vector2 velocity) => _movement.SetVelocity(velocity);

    // Входные методы из инпута
    public void PerformAttack(Vector2 direction)
    {
        Debug.Log($"Attack attempt: state={_state}, atkCd={_attackCooldownLeft}");
        if (_state is PlayerState.Attacking or PlayerState.Dashing or PlayerState.Parrying) return;
        if (_attackCooldownLeft > 0) return;

        _state = PlayerState.Attacking;
        _attackCooldownLeft = _attackData.AttackCooldown;
        if (direction != Vector2.zero) _lastDirection = direction.normalized;

        // Берем верхнюю (последнюю добавленную) основную атаку или базовую
        IAttack main = (_mainAttackSet != null && _mainAttackSet.Count > 0) ? _mainAttackSet[_mainAttackSet.Count - 1] : _mainAttack;
        main?.PerformAttack(_lastDirection);

        Debug.Log($"Attack executed: dir={_lastDirection}, cd={_attackCooldownLeft}");
    }

    public void PerformAirAttack(Vector2 direction)
    {
        if (_movement.IsGrounded() || _state is PlayerState.Attacking or PlayerState.Dashing or PlayerState.Parrying || _attackCooldownLeft > 0) return;
        _state = PlayerState.Attacking;
        _attackCooldownLeft = _attackData.AttackCooldown;
        if (direction != Vector2.zero) _lastDirection = direction.normalized;
        _downAttack.PerformAttack(_lastDirection);
    }

    public void PerformParry()
    {
        Debug.Log($"Parry attempt: state={_state}");
        if (_state is PlayerState.Attacking or PlayerState.Dashing or PlayerState.Parrying) return;

        _state = PlayerState.Parrying;
        if (_currentParryCoroutine != null) StopCoroutine(_currentParryCoroutine);
        _currentParryCoroutine = StartCoroutine(ParryTimer());
        _parryAttack.PerformAttack(Vector2.zero);
        Debug.Log("Parry executed");
    }

    private IEnumerator ParryTimer()
    {
        yield return new WaitForSeconds(0.5f);
        _state = PlayerState.Idle;
        _currentParryCoroutine = null;
        Debug.Log("Parry state reset");
    }

    // Слоты способностей
    public void UseFood(int slot)
    {
        if (_abilitiesSet != null && slot >= 0 && slot < _abilitiesSet.Count && _abilitiesSet[slot] != null)
        {
            Debug.Log($"Use ability slot={slot}");
            _abilitiesSet[slot].PerformAttack(_lastDirection);
            return;
        }
        Debug.Log($"Use food: slot={slot} has no ability");
    }

    // Заполнение наборов атак из биндеров/загрузчиков
    public void SetMainAttackSet(List<IAttack> attacks)
    {
        _mainAttackSet = attacks ?? new List<IAttack>();
    }

    public void SetAbilitiesSet(List<IAttack> abilities)
    {
        _abilitiesSet = abilities ?? new List<IAttack>();
    }

    // Обратная совместимость: методы экипировки обновляют списки
    public void EquipLmbOverride(IAttack ability)
    {
        // Добавляем как верхнюю основную атаку
        if (ability != null)
            _mainAttackSet.Add(ability);
    }
    public void EquipQ(IAttack ability)
    {
        while (_abilitiesSet.Count <= 0) _abilitiesSet.Add(null);
        _abilitiesSet[0] = ability;
    }
    public void EquipE(IAttack ability)
    {
        while (_abilitiesSet.Count <= 1) _abilitiesSet.Add(null);
        _abilitiesSet[1] = ability;
    }

    public float GetDamage() => _attackData.BaseDamage;
    public float GetAttackRadius() => _attackData.AttackRadius;

    public void ResetAllStates()
    {
        _state = PlayerState.Idle;
        _dashTimeLeft = 0f;
        _dashCooldownLeft = 0f;
        _attackCooldownLeft = 0f;
        if (_currentParryCoroutine != null)
        {
            StopCoroutine(_currentParryCoroutine);
            _currentParryCoroutine = null;
        }
        Debug.Log("All player states have been reset");
    }

    // IHittable / IHealable
    public void TakeDamage(float amount)
    {
        if (_state == PlayerState.Parrying) return;
        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.CeilToInt(amount));
        if (_currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        int newHp = _currentHealth + Mathf.CeilToInt(amount);
        _currentHealth = Mathf.Clamp(newHp, 0, _playerData != null ? _playerData.MaxHealth : 100);
    }

    public float GetCurrentHealth() => _currentHealth;
    public float GetMaxHealth() => _playerData != null ? _playerData.MaxHealth : 100;
    public bool IsAlive() => _currentHealth > 0;

    public void Die()
    {
        Debug.Log("Player died!");
    }

    // Выдача дополнительных прыжков от навыков/еды
    public void SetExtraJumps(int extra)
    {
        _movement.SetExtraJumps(extra);
    }

    private void OnDrawGizmos() { }
}
