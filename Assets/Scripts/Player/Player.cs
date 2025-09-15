using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour, IHittable, IHealable, IEffectHandler
{
    [Header("Компоненты")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GroundChecker _groundChecker;
    [SerializeField] private Transform _dialogueBubblePos;
    [SerializeField] private Transform _cameraTarget;

    public Transform CameraTarget => _cameraTarget;

    // Данные игрока
    private PlayerDataSO _playerData;
    private MoveDataSO _moveData;

    // Input через Unity PlayerInput component
    private PlayerInput _playerInput;
    private IMovable _movement;

    // Состояния
    public enum PlayerState { Idle, Moving, Jumping, Dashing, Attacking, Parrying }
    private PlayerState _state = PlayerState.Idle;
    
    private List<IAttack> _mainAttackSet = new(3);
    private readonly List<IAttack> _abilitiesSet = new(3);
    private readonly List<EffectBase> _activeEffects = new();

    private Coroutine _foodDeactivationRoutine;
    
    private CurrentPlayerData _data;

    public PlayerState State => _state;
    public CurrentPlayerData Data => _data;
    public Transform DialogueBubblePos => _dialogueBubblePos;
    public PlayerMovementLogic Movement => (PlayerMovementLogic)_movement;

    private DiContainer _container;

    [Header("Debug/Visuals")]
    [SerializeField] private bool _showAttackGizmos = true;

    [Inject]
    private void Construct(PlayerDataSO playerData, MoveDataSO moveData, PlayerInput input, DiContainer container)
    {
        _playerData = playerData;
        _moveData = moveData;
        _playerInput = input; // DI через Zenject
        _container = container;
    }

    private void Awake()
    {
        // _playerInput приходит из DI, оставляем фолбэк на случай отсутствия компонента
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
                Debug.LogError("Player: Unity PlayerInput component is missing. Please add PlayerInput to the Player GameObject.");
        }
        if (_moveData == null)
        {
            Debug.LogError("Player: MoveDataSO is null. Assign it in GameplayInstaller.");
        }
        else
        {
            _movement = new PlayerMovementLogic(_moveData, _rigidbody);
            // Подключаем события ввода к логике движения
            Movement.Initialize(this, _playerInput);
        }
        if (_playerData == null)
        {
            Debug.LogError("Player: PlayerDataSO is null. Assign it in GameplayInstaller.");
        }
        else
        {
            _mainAttackSet = _playerData.AttackSet;
            // ВАЖНО: инъекция зависимостей в объекты атак (Player, PlayerInput и т.д.)
            if (_container != null && _mainAttackSet != null)
            {
                foreach (var attack in _mainAttackSet)
                {
                    if (attack != null)
                        _container.Inject(attack);
                }
            }
        }
 
        // Отладка данных
        if (_moveData != null)
            Debug.Log($"MoveData: speed={_moveData.MoveSpeed}, accel={_moveData.Acceleration}, jumpForce={_moveData.JumpForce}");
 
        // Инициализация здоровья (если не загружено сохранение)
        if (_playerData != null)
        {
            if (_data.Health <= 0 || _data.Health > _playerData.MaxHealth)
                _data.Health = _playerData.MaxHealth;
        }

        // Принудительно включаем карту действий Player (если активна не она)
        if (_playerInput != null)
        {
            var mapName = _playerInput.currentActionMap != null ? _playerInput.currentActionMap.name : "(null)";
            Debug.Log($"PlayerInput current map: {mapName}");
            if (_playerInput.currentActionMap == null || _playerInput.currentActionMap.name != "Player")
            {
                _playerInput.SwitchCurrentActionMap("Player");
                Debug.Log("PlayerInput: switched current action map to 'Player'");
            }
        }

        // Диагностика AttackSet
        Debug.Log($"AttackSet count: {_mainAttackSet?.Count ?? -1}");
        if (_mainAttackSet != null)
        {
            for (int i = 0; i < _mainAttackSet.Count; i++)
            {
                var a = _mainAttackSet[i];
                Debug.Log($"Attack[{i}]: {a?.GetType().Name} binding='{a?.Data?.InputBinding}'");
            }
        }
    }

    private void OnEnable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged += OnGroundChanged;
        // Активируем основные атаки игрока, чтобы вернулась логика ударов
        if (_mainAttackSet != null)
        {
            foreach (var attack in _mainAttackSet)
                attack?.Activate();
        }
        if (_playerInput != null)
        {
            _playerInput.actions["1"].performed += OnFood1;
            _playerInput.actions["2"].performed += OnFood2;
            _playerInput.actions["3"].performed += OnFood3;
        }
    }

    private void OnDisable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged -= OnGroundChanged;
        // Деактивируем основные атаки
        if (_mainAttackSet != null)
        {
            foreach (var attack in _mainAttackSet)
                attack?.Deactivate();
        }
        if (_playerInput != null)
        {
            _playerInput.actions["1"].performed -= OnFood1;
            _playerInput.actions["2"].performed -= OnFood2;
            _playerInput.actions["3"].performed -= OnFood3;
        }
    }

    private void FixedUpdate()
    {
        if (_playerInput == null) return;
        float x = 0f;
        var moveAction = _playerInput.actions["Move"];
        if (moveAction != null)
            x = moveAction.ReadValue<float>(); // 1D Axis (A/D)
        Movement.Move(new Vector2(x, 0f));
    }

    private void OnGroundChanged(bool grounded)
    {
        Movement.UpdateGrounded(grounded);
        if (grounded && _state == PlayerState.Jumping)
        {
            _state = PlayerState.Idle;
        }
        Debug.Log($"Ground state changed: {grounded}");
    }


    private void AddAbility(int toSlot, IAttack food, int foodId)
    {
        if (_abilitiesSet[toSlot] != null)
        {
            _data.InventoryFood.Add(_data.UsedFood[toSlot]);
        }
        _abilitiesSet[toSlot] = food;
        _data.UsedFood[toSlot] = foodId;
    }
    
    private void MoveAbility(IAttack inventFood, int inventFoodId, IAttack useFood, int useFoodId)
    {
        
    }
    
    private void SwitchAbility(int fromSlot, int toSlot)
    {
        (_abilitiesSet[fromSlot], _abilitiesSet[toSlot]) = (_abilitiesSet[toSlot], _abilitiesSet[fromSlot]);
    }
    

    // Слоты способностей
    public void UseFood(int slot)
    {
        if (_abilitiesSet == null || slot < 0 || slot >= _abilitiesSet.Count || _abilitiesSet[slot] == null) return;
        Debug.Log($"Use ability slot={slot}");
        _abilitiesSet[slot].Activate();
        var mainToDeactivate = _mainAttackSet.Find(attack => attack.Data.InputBinding == _abilitiesSet[slot].Data.InputBinding);
        if (mainToDeactivate != null)
        {
            mainToDeactivate.Deactivate();
            _foodDeactivationRoutine = StartCoroutine(FoodDeactivationRoutine(_abilitiesSet[slot], mainToDeactivate));
        }
        else
        {
            _foodDeactivationRoutine = StartCoroutine(FoodDeactivationRoutine(_abilitiesSet[slot]));
        }
    }

    private IEnumerator FoodDeactivationRoutine(IAttack data)
    {
        yield return new WaitForSeconds(((FoodData)data.Data).Duration);
        data.Deactivate();
    }
    
    private IEnumerator FoodDeactivationRoutine(IAttack data, IAttack dataToActivate)
    {
        yield return new WaitForSeconds(((FoodData)data.Data).Duration);
        data.Deactivate();
        dataToActivate.Activate();
    }

    // Управление эффектами
    public void AddEffect(EffectBase effectBase)
    {
        if (effectBase != null && !_activeEffects.Contains(effectBase))
        {
            _activeEffects.Add(effectBase);
            effectBase.ApplyEffect(gameObject);
        }
    }

    public void RemoveEffect(EffectBase effectBase)
    {
        if (effectBase != null && _activeEffects.Contains(effectBase))
        {
            _activeEffects.Remove(effectBase);
            effectBase.RemoveEffect(gameObject);
        }
    }
    
    public void TakeDamage(int amount)
    {
        if (_state == PlayerState.Parrying) return;
        _data.Health = Mathf.Max(0, _data.Health - Mathf.CeilToInt(amount));
        if (_data.Health <= 0) Die();
    }
    
    public void Die()
    {
        Debug.Log("Player died!");
    }
    
    public bool TryBuy(int price)
    {
        if (_data.Money < price) return false;
        _data.Money -= price;
        return true;
    }

    public void Heal(int amount)
    {
        int newHp = _data.Health + Mathf.CeilToInt(amount);
        _data.Health = Mathf.Clamp(newHp, 0, _playerData != null ? _playerData.MaxHealth : 100);
    }

    // Input handlers for food hotkeys (so we can unsubscribe correctly)
    private void OnFood1(InputAction.CallbackContext ctx) => UseFood(0);
    private void OnFood2(InputAction.CallbackContext ctx) => UseFood(1);
    private void OnFood3(InputAction.CallbackContext ctx) => UseFood(2);

    private void OnDrawGizmosSelected()
    {
        if (!_showAttackGizmos) return;
        // Берём список атак: в рантайме — _mainAttackSet, в редакторе — из _playerData (если есть)
        var attacks = _mainAttackSet ?? _playerData?.AttackSet;
        if (attacks == null) return;

        Vector2 face = Vector2.right;
        if (_movement is PlayerMovementLogic m)
        {
            var d = m.LastDirection;
            if (d != Vector2.zero) face = d.normalized;
        }

        var pos = (Vector2)transform.position;
        foreach (var attack in attacks)
        {
            if (attack == null || attack.Data == null) continue;

            // Вперёд (ЛКМ) — MainAttackLogic с MainAttackData
            if (attack is MainAttackLogic)
            {
                var data = attack.Data as Core.Data.ScriptableObjects.MainAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Vector2 center = pos + face * radius * data.ForwardOffset;
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(center, radius);
                continue;
            }

            // Вниз (S) — DownAttackLogic с MainAttackData
            if (attack is DownAttackLogic)
            {
                var data = attack.Data as Core.Data.ScriptableObjects.MainAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Vector2 center = pos + Vector2.down * radius * data.ForwardOffset;
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(center, radius);
                continue;
            }

            // Парирование (ПКМ) — ParryAttackLogic с ParryAttackData
            if (attack is ParryAttackLogic)
            {
                var data = attack.Data as Core.Data.ScriptableObjects.ParryAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pos, radius);
            }
        }
    }
}