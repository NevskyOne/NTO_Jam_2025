using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour, IHittable, IHealable
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
    private Vector2 _cachedMove;

    // Состояния
    public enum PlayerState { Idle, Moving, Jumping, Dashing, Attacking, Parrying }
    private PlayerState _state = PlayerState.Idle;
    
    private readonly List<IAttack> _mainAttackSet = new();
    private readonly List<IAttack> _abilitiesSet = new();
    private readonly List<IEffect> _activeEffects = new();

    private Coroutine _foodDeactivationRoutine;
    
    private CurrentPlayerData _data;

    public PlayerState State => _state;
    public Transform DialogueBubblePos => _dialogueBubblePos;
    public PlayerMovementLogic Movement => (PlayerMovementLogic)_movement;

    [Inject]
    private void Construct(PlayerDataSO playerData, MoveDataSO moveData, PlayerInput input)
    {
        _playerData = playerData;
        _moveData = moveData;
        _playerInput = input;
    }

    private void Awake()
    {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_groundChecker == null) _groundChecker = GetComponentInChildren<GroundChecker>();
        _playerInput = GetComponent<PlayerInput>();

        _movement = new PlayerMovementLogic(_moveData, _rigidbody);

        // Отладка данных
        Debug.Log($"MoveData: speed={_moveData.MoveSpeed}, accel={_moveData.Acceleration}, jumpForce={_moveData.JumpForce}");
        
        _mainAttackSet.Add(new MainAttackLogic());
        _mainAttackSet.Add(new DownAttackLogic());
        _mainAttackSet.Add(new ParryAttackLogic());
        
    }

    private void OnEnable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged += OnGroundChanged;

        _playerInput.actions["1"].performed += _ => UseFood(0);
        _playerInput.actions["2"].performed += _ => UseFood(1);
        _playerInput.actions["3"].performed += _ => UseFood(2);
    }

    private void OnDisable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged -= OnGroundChanged;
        
        _playerInput.actions["1"].performed -= _ => UseFood(0);
        _playerInput.actions["2"].performed -= _ => UseFood(1);
        _playerInput.actions["3"].performed -= _ => UseFood(2);
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

    public void Heal(float amount)
    {
        int newHp = _data.Health + Mathf.CeilToInt(amount);
        _data.Health = Mathf.Clamp(newHp, 0, _playerData != null ? _playerData.MaxHealth : 100);
    }
}