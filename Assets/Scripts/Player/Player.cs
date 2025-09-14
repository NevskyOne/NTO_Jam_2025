using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;

public enum FoodType
{
    None,
    Tea,
    IcedLatte,
    DragonFruit,
    Dumpling,
    KoreanCarrot,
    Ratatouille,
    Burger,
    ExplosiveCaramel,
    PoisonPotato
}

[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public class Player : MonoBehaviour, IMovable, IHittable, IHealable
{
    [Header("Компоненты")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GroundChecker _groundChecker;

    // Данные игрока
    private PlayerDataSO _playerData;
    private AttackDataSO _attackData;
    private MoveDataSO _moveData;

    // Input через Unity PlayerInput component
    private UnityEngine.InputSystem.PlayerInput _playerInput;
    private IMovable _movement;
    private IAttack _mainAttack;
    private IAttack _downAttack;
    private IAttack _parryAttack;
    private Vector2 _cachedMove;

    // Состояния
    private enum PlayerState { Idle, Moving, Jumping, Dashing, Attacking, Parrying }
    private PlayerState _state = PlayerState.Idle;
    private Vector2 _lastDirection = Vector2.right;

    // Наборы атак
    private List<IAttack> _mainAttackSet = new();
    public List<IAttack> AbilitiesSet = new();
    
    // Эффекты
    private List<IEffect> _activeEffects = new();

    // Runtime-значения PlayerData
    private int _currentHealth;
    private int _currentMoney;
    private int _currentReputation;

    // Способности из биндера (перенесено из PlayerAbilitiesBinder)
    [Header("Loadout")]
    [SerializeField] private FoodType _lmbOverride = FoodType.None;
    [SerializeField] private FoodType _qAbility = FoodType.None;
    [SerializeField] private FoodType _eAbility = FoodType.None;
    [Header("Passive/Always On")]
    [SerializeField] private FoodType _passive = FoodType.None;

    [SerializeField] private Transform _dialogueBubblePos;
    public Transform DialogueBubblePos => _dialogueBubblePos;

    [Inject]
    private void Construct(PlayerDataSO playerData, AttackDataSO attackData, MoveDataSO moveData)
    {
        _playerData = playerData;
        _attackData = attackData;
        _moveData = moveData;
    }

    private void Awake()
    {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_groundChecker == null) _groundChecker = GetComponentInChildren<GroundChecker>();
        _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();

        // Отладка компонентов
        Debug.Log($"Player Awake: rigidbody={_rigidbody != null}, groundChecker={_groundChecker != null}");
        if (_rigidbody != null)
        {
            Debug.Log($"Rigidbody2D: mass={_rigidbody.mass}, drag={_rigidbody.linearDamping}, freezePositionX={_rigidbody.freezeRotation}");
            Debug.Log($"Rigidbody2D constraints: {_rigidbody.constraints}");
        }

        // Проверяем наличие DownAttack действия в Input Action Asset
        if (_playerInput != null && _playerInput.actions != null)
        {
            var downAttackAction = _playerInput.actions.FindAction("DownAttack");
            Debug.Log($"DownAttack action found: {downAttackAction != null}");
            if (downAttackAction != null)
            {
                Debug.Log($"DownAttack bindings: {downAttackAction.bindings.Count}");
                foreach (var binding in downAttackAction.bindings)
                {
                    Debug.Log($"DownAttack binding: {binding.path}");
                }
            }
        }

        _movement = new PlayerMovementLogic(_moveData, _rigidbody);
        _mainAttack = new MainAttackLogic(_attackData, transform);
        _downAttack = new DownAttackLogic(_attackData, transform);
        _parryAttack = new ParryAttackLogic(_attackData, transform);

        // Отладка данных
        if (_moveData != null)
        {
            Debug.Log($"MoveData: speed={_moveData.MoveSpeed}, accel={_moveData.Acceleration}, jumpForce={_moveData.JumpForce}");
        }
        else
        {
            Debug.LogError("MoveDataSO is NULL! Check Zenject bindings in GameplayInstaller");
        }

        // Инициализируем набор основной атаки базовой атакой
        _mainAttackSet.Clear();
        _mainAttackSet.Add(_mainAttack);
        
        // Инициализация способностей (перенесено из PlayerAbilitiesBinder)
        InitializeAbilities();

        _currentHealth = _playerData != null ? _playerData.MaxHealth : 100;
        _currentMoney = _playerData != null ? _playerData.StartMoney : 0;
        _currentReputation = _playerData != null ? _playerData.StartReputation : 0;
    }

    private void OnEnable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged += OnGroundChanged;
    }

    private void OnDisable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged -= OnGroundChanged;
    }

    private void OnGroundChanged(bool grounded)
    {
        _movement.UpdateGrounded(grounded);
        if (grounded && _state == PlayerState.Jumping)
        {
            _state = PlayerState.Idle;
        }
        Debug.Log($"Ground state changed: {grounded}");
    }

    private void Update()
    {
        HandleMovement();
        HandleState();
        
        // Обновляем кулдауны атак
        ((MainAttackLogic)_mainAttack).UpdateCooldown();
        ((DownAttackLogic)_downAttack).UpdateCooldown();
        ((ParryAttackLogic)_parryAttack).UpdateCooldown();
    }

    private void HandleMovement()
    {
        // Замедляем движение во время атак
        Vector2 moveInput = _cachedMove;
        if (_state == PlayerState.Attacking || _state == PlayerState.Parrying)
        {
            moveInput *= 0.3f; // Замедление на 70% во время атак
        }
        _movement.Move(moveInput, Time.deltaTime);
    }

    private void HandleState()
    {
        if (_state == PlayerState.Attacking)
        {
            if (_mainAttack != null)
            {
                if (_mainAttack.GetAttackDuration() <= 0)
                {
                    _state = PlayerState.Idle;
                    Debug.Log("Attack state ended");
                }
            }
        }
        else if (_state == PlayerState.Parrying)
        {
            if (_parryAttack != null)
            {
                if (_parryAttack.GetAttackDuration() <= 0)
                {
                    _state = PlayerState.Idle;
                    Debug.Log("Parry state ended");
                }
            }
        }
        else if (_state == PlayerState.Dashing)
        {
            if (_movement != null)
            {
                if (_movement.GetDashDuration() <= 0)
                {
                    _state = PlayerState.Idle;
                    Debug.Log("Dash state ended");
                }
            }
            
            // Во время dash разрешаем ограниченное движение
            _movement.Move(_cachedMove, Time.deltaTime);
        }
        else
        {
            _state = Mathf.Abs(_cachedMove.x) > 0.01f ? PlayerState.Moving : PlayerState.Idle;
            if (_cachedMove.x != 0) _lastDirection = new Vector2(Mathf.Sign(_cachedMove.x), 0);
        }
        
        // Отладка движения
        if (Time.frameCount % 60 == 0 && _cachedMove != Vector2.zero)
        {
            Debug.Log($"Movement Debug: cachedMove={_cachedMove}, velocity={_rigidbody.linearVelocity}, state={_state}");
        }
    }

    // Send Messages callbacks (вызываются автоматически PlayerInput компонентом)
    public void OnMove(InputValue value)
    {
        float horizontal = value.Get<float>();
        _cachedMove = new Vector2(horizontal, 0f);
        Debug.Log($"OnMove: {_cachedMove}");
    }

    public void OnJump(InputValue value) 
    { 
        if (value.isPressed && _state != PlayerState.Attacking && _state != PlayerState.Parrying && _state != PlayerState.Dashing) 
        {
            if (_movement.TryJump())
            {
                _state = PlayerState.Jumping;
                Debug.Log("Jump executed");
            }
        }
    }
    
    public void OnShift(InputValue value) 
    { 
        if (value.isPressed && _state != PlayerState.Dashing && _state != PlayerState.Attacking && _state != PlayerState.Parrying) 
        {
            Vector2 direction = _cachedMove.normalized;
            if (direction == Vector2.zero) direction = _lastDirection;
            _movement.Dash(direction);
            _state = PlayerState.Dashing;
            Debug.Log($"Dash executed: {direction}");
        }
    }
    
    public void OnLeftMouse(InputValue value) 
    { 
        if (value.isPressed && _state != PlayerState.Attacking && _state != PlayerState.Dashing && _state != PlayerState.Parrying) 
        {
            // Запрещаем атаку в воздухе
            if (!_movement.IsGrounded())
            {
                Debug.Log("Cannot attack while in air");
                return;
            }
            
            // Проверяем кулдаун основной атаки
            IAttack main = (_mainAttackSet != null && _mainAttackSet.Count > 0) ? _mainAttackSet[_mainAttackSet.Count - 1] : _mainAttack;
            if (main != null && main.GetAttackDuration() > 0)
            {
                Debug.Log("Main attack on cooldown");
                return;
            }
            
            // Берем верхнюю (последнюю добавленную) основную атаку или базовую
            main?.PerformAttack(_lastDirection);
            _state = PlayerState.Attacking;
            Debug.Log("Main attack executed");
        }
    }

    public void OnRightMouse(InputValue value) 
    { 
        if (value.isPressed && _state != PlayerState.Attacking && _state != PlayerState.Dashing && _state != PlayerState.Parrying) 
        {
            // Запрещаем парирование в воздухе
            if (!_movement.IsGrounded())
            {
                Debug.Log("Cannot parry while in air");
                return;
            }
            
            // Проверяем кулдаун парирования
            if (_parryAttack.GetAttackDuration() > 0)
            {
                Debug.Log("Parry on cooldown");
                return;
            }
            
            _parryAttack.PerformAttack(Vector2.zero);
            _state = PlayerState.Parrying;
            Debug.Log("Parry executed");
        }
    }

    public void OnQ(InputValue value) { if (value.isPressed) UseFood(0); }
    public void OnE(InputValue value) { if (value.isPressed) UseFood(1); }
    public void OnDownAttack(InputValue value) 
    { 
        Debug.Log($"OnDownAttack called: isPressed={value.isPressed}, isGrounded={_movement.IsGrounded()}, state={_state}");
        
        if (value.isPressed && !_movement.IsGrounded()) 
        {
            Debug.Log($"DownAttack conditions met, checking cooldown...");
            
            // Проверяем кулдаун атаки вниз
            float cooldown = _downAttack.GetAttackDuration();
            Debug.Log($"DownAttack cooldown: {cooldown}");
            
            if (_downAttack.GetAttackDuration() > 0)
            {
                Debug.Log("Down attack on cooldown");
                return;
            }
            
            Debug.Log("Executing DownAttack...");
            _downAttack.PerformAttack(_lastDirection);
            _state = PlayerState.Attacking;
            Debug.Log("Down attack executed");
        }
        else
        {
            Debug.Log($"DownAttack conditions NOT met: isPressed={value.isPressed}, isGrounded={_movement.IsGrounded()}");
        }
    }

    // Переключение способностей
    public void On1(InputValue value) { if (value.isPressed) SwitchAbility(0); }
    public void On2(InputValue value) { if (value.isPressed) SwitchAbility(1); }
    public void On3(InputValue value) { if (value.isPressed) SwitchAbility(2); }
    
    private void SwitchAbility(int slot)
    {
        Debug.Log($"Switch to ability slot {slot}");
        // Здесь может быть логика переключения активной способности
        // Например, изменение UI или подготовка к использованию способности
    }

    // Делегирование к IMovable
    public bool IsGrounded() => _movement.IsGrounded();
    public Vector2 GetVelocity() => _movement.GetVelocity();
    public void SetVelocity(Vector2 velocity) => _movement.SetVelocity(velocity);
    public void UpdateGrounded(bool isGrounded) => _movement.UpdateGrounded(isGrounded);
    public float GetDashDuration() => _movement.GetDashDuration();
    public void Move(Vector2 direction, float deltaTime) => _movement.Move(direction, deltaTime);
    public void Jump() => _movement.Jump();
    public bool TryJump() => _movement.TryJump();
    public void Dash(Vector2 direction) => _movement.Dash(direction);

    // Слоты способностей
    public void UseFood(int slot)
    {
        if (AbilitiesSet != null && slot >= 0 && slot < AbilitiesSet.Count && AbilitiesSet[slot] != null)
        {
            Debug.Log($"Use ability slot={slot}");
            AbilitiesSet[slot].PerformAttack(_lastDirection);
            
            // Удаляем блюдо после использования (одноразовое использование)
            AbilitiesSet[slot] = null;
            Debug.Log($"Food consumed from slot {slot}");
            return;
        }
        Debug.Log($"Use food: slot={slot} has no ability");
    }

    // Инициализация способностей (перенесено из PlayerAbilitiesBinder)
    private void InitializeAbilities()
    {
        EquipLmbOverride(CreateAbility(_lmbOverride));
        EquipQ(CreateAbility(_qAbility));
        EquipE(CreateAbility(_eAbility));
        ApplyPassive(_passive);
    }

    private IAttack CreateAbility(FoodType type)
    {
        switch (type)
        {
            case FoodType.Tea: return new Abilities.Food.TeaAbility(_attackData, transform);
            case FoodType.IcedLatte: return new Abilities.Food.IcedLatteAbility(_attackData, transform);
            case FoodType.DragonFruit: return new Abilities.Food.DragonFruitAbility(_attackData, transform);
            case FoodType.Dumpling: return new Abilities.Food.DumplingAbility(_attackData, transform);
            case FoodType.KoreanCarrot: return new Abilities.Food.KoreanCarrotAbility(_attackData, transform);
            case FoodType.Ratatouille: return new Abilities.Food.RatatouilleAbility(_attackData, transform);
            case FoodType.Burger: return new Abilities.Food.BurgerAbility(_attackData, transform);
            case FoodType.ExplosiveCaramel: return new Abilities.Food.ExplosiveCaramelAbility(_attackData, transform);
            case FoodType.PoisonPotato: return new Abilities.Food.PoisonPotatoAbility(_attackData, transform);
            default: return null;
        }
    }

    private void ApplyPassive(FoodType type)
    {
        if (type == FoodType.KoreanCarrot)
        {
            // SetExtraJumps(1);
        }
    }

    // Заполнение наборов атак из биндеров/загрузчиков
    public void SetMainAttackSet(List<IAttack> attacks)
    {
        _mainAttackSet = attacks ?? new List<IAttack>();
    }

    public void SetAbilitiesSet(List<IAttack> abilities)
    {
        AbilitiesSet = abilities ?? new List<IAttack>();
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
        while (AbilitiesSet.Count <= 0) AbilitiesSet.Add(null);
        AbilitiesSet[0] = ability;
    }
    public void EquipE(IAttack ability)
    {
        while (AbilitiesSet.Count <= 1) AbilitiesSet.Add(null);
        AbilitiesSet[1] = ability;
    }

    // Управление эффектами
    public void AddEffect(IEffect effect)
    {
        if (effect != null && !_activeEffects.Contains(effect))
        {
            _activeEffects.Add(effect);
            effect.ApplyEffect();
        }
    }

    public void RemoveEffect(IEffect effect)
    {
        if (effect != null && _activeEffects.Contains(effect))
        {
            _activeEffects.Remove(effect);
            effect.RemoveEffect();
        }
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
}