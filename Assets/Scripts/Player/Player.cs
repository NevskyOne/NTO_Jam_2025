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

    [Inject]
    private void Construct(PlayerDataSO playerData, MoveDataSO moveData)
    {
        _playerData = playerData;
        _moveData = moveData;
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("Player: Unity PlayerInput component is missing. Please add PlayerInput to the Player GameObject.");
        }
        _movement = new PlayerMovementLogic(_moveData, _rigidbody);
        // Подключаем события ввода к логике движения без DI
        Movement.Initialize(this, _playerInput);
        _mainAttackSet = _playerData.AttackSet;

        // Отладка данных
        Debug.Log($"MoveData: speed={_moveData.MoveSpeed}, accel={_moveData.Acceleration}, jumpForce={_moveData.JumpForce}");
        
    }

    private void OnEnable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged += OnGroundChanged;
        if (_playerInput != null)
        {
            _playerInput.actions["1"].performed += _ => UseFood(0);
            _playerInput.actions["2"].performed += _ => UseFood(1);
            _playerInput.actions["3"].performed += _ => UseFood(2);
        }
    }

    private void OnDisable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged -= OnGroundChanged;
        if (_playerInput != null)
        {
            _playerInput.actions["1"].performed -= _ => UseFood(0);
            _playerInput.actions["2"].performed -= _ => UseFood(1);
            _playerInput.actions["3"].performed -= _ => UseFood(2);
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
}