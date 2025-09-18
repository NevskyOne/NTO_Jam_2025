using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour, IHittable, IHealable, IEffectHandler
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private GroundChecker _groundChecker;
    [SerializeField] private InteractableTrigger _interactableTrigger;
    [SerializeField] private Transform _dialogueBubblePos;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private ShadowCaster2D _shadowCaster;
    
    public ShadowCaster2D ShadowCaster => _shadowCaster;
    public Transform CameraTarget => _cameraTarget;
    
    private PlayerDataSO _playerData;
    private MoveDataSO _moveData;
    private DiContainer _container;
    
    private PlayerInput _playerInput;
    private IMovable _movement;
    
    public enum PlayerState { Idle, Moving, Jumping, Dashing, Attacking, Parrying }
    private PlayerState _state = PlayerState.Idle;
    
    private List<IAttack> _mainAttackSet = new(3);
    private readonly List<IAttack> _abilitiesSet = new(3);
    private readonly List<EffectBase> _activeEffects = new();

    private Coroutine _foodDeactivationRoutine;
    
    private CurrentPlayerData _data = new CurrentPlayerData();
    private int _currentPills;

    private IInteractable _currentInteractable;
    public PlayerState State => _state;
    public CurrentPlayerData Data => _data;
    public Transform DialogueBubblePos => _dialogueBubblePos;
    public PlayerMovementLogic Movement => (PlayerMovementLogic)_movement;
    
    [Header("Debug/Visuals")]
    [SerializeField] private bool _showAttackGizmos = true;

    [Inject]
    private void Construct(PlayerDataSO playerData, MoveDataSO moveData, PlayerInput input, PlayerMovementLogic movement, DiContainer container)
    {
        
        _playerData = playerData;
        _moveData = moveData;
        _playerInput = input; 
        _container = container;

        _movement = movement;
    }

    private void Awake()
    {
        // Защита от NullReferenceException
        if (_playerData == null) 
        {
            Debug.LogError("[Player] _playerData не назначен. Проверьте ссылку в GameplayInstaller.");
            return;
        }

        _mainAttackSet = _playerData.AttackSet;
        if (_mainAttackSet != null && _container != null)
        {
            foreach (var attack in _mainAttackSet)
            {
                if (attack != null)
                    _container.Inject(attack);
            }
        }
        
        if (_moveData != null)
            Debug.Log($"MoveData: speed={_moveData.MoveSpeed}, accel={_moveData.Acceleration}, jumpForce={_moveData.JumpForce}");
        
        if (_playerData != null)
        {
            if (_data.Health <= 0 || _data.Health > _playerData.MaxHealth)
                _data.Health = _playerData.MaxHealth;
        }

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
     
        if (_mainAttackSet != null)
        {
            foreach (var attack in _mainAttackSet)
                attack?.Activate();
        }

        _interactableTrigger.FindInter += inter => _currentInteractable = inter;
        _interactableTrigger.LostInter += () =>  _currentInteractable = null;
        if (_playerInput != null)
        {
            _playerInput.actions["1"].performed += OnFood1;
            _playerInput.actions["2"].performed += OnFood2;
            _playerInput.actions["3"].performed += OnFood3;
            _playerInput.actions["E"].performed += InteractWith;
            _playerInput.actions["H"].performed += _ => Heal(1);
        }
    }

    private void OnDisable()
    {
        if (_groundChecker != null)
            _groundChecker.GroundStateChanged -= OnGroundChanged;
      
        if (_mainAttackSet != null)
        {
            foreach (var attack in _mainAttackSet)
                attack?.Deactivate();
        }
        _interactableTrigger.FindInter -= inter => _currentInteractable = inter;
        _interactableTrigger.LostInter -= () =>  _currentInteractable = null;
        if (_playerInput != null)
        {
            _playerInput.actions["1"].performed -= OnFood1;
            _playerInput.actions["2"].performed -= OnFood2;
            _playerInput.actions["3"].performed -= OnFood3;
            _playerInput.actions["E"].performed -= InteractWith;
            _playerInput.actions["H"].performed -= _ => Heal(1);
        }
    }

    private void InteractWith(InputAction.CallbackContext ctx)
    {
        _currentInteractable?.Interact();
    }

    private void OnGroundChanged(bool grounded)
    {
        Movement.UpdateGrounded(grounded);
        if(grounded) ((DownAttackLogic)_mainAttackSet[1]).Attacked = false;
        if (grounded && _state == PlayerState.Jumping)
        {
            _state = PlayerState.Idle;
        }
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
    
    
    public void UseFood(int slot)
    {
        Debug.Log($"🍽️ [UseFood] Попытка использовать еду в слоте {slot}");
        
        if (_abilitiesSet == null)
        {
            Debug.LogError("🍽️ [UseFood] _abilitiesSet is null!");
            return;
        }
        
        if (slot < 0 || slot >= _abilitiesSet.Count)
        {
            Debug.LogError($"🍽️ [UseFood] Неверный слот {slot}. Доступно слотов: {_abilitiesSet.Count}");
            return;
        }
        
        if (_abilitiesSet[slot] == null)
        {
            Debug.LogError($"🍽️ [UseFood] Еда в слоте {slot} равна null!");
            return;
        }
        
        Debug.Log($"🍽️ [UseFood] Активируем способность в слоте {slot}: {_abilitiesSet[slot].GetType().Name}");
        _abilitiesSet[slot].Activate();
        
        var mainToDeactivate = _mainAttackSet.Find(attack => attack.Data.InputBinding == _abilitiesSet[slot].Data.InputBinding);
        if (mainToDeactivate != null)
        {
            Debug.Log($"🍽️ [UseFood] Деактивируем основную атаку: {mainToDeactivate.GetType().Name}");
            mainToDeactivate.Deactivate();
            _foodDeactivationRoutine = StartCoroutine(FoodDeactivationRoutine(_abilitiesSet[slot], mainToDeactivate));
        }
        else
        {
            Debug.Log($"🍽️ [UseFood] Основная атака для деактивации не найдена");
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

    public void AddPill() => _currentPills += 1;
    
    public void Heal(int amount)
    {
        if (_currentPills <= 0) return;
        int newHp = _data.Health + Mathf.CeilToInt(amount);
        _data.Health = Mathf.Clamp(newHp, 0, _playerData.MaxHealth);
        _currentPills -= 1;
    }
    
    private void OnFood1(InputAction.CallbackContext ctx) 
    { 
        Debug.Log("🍽️ [Input] Нажата клавиша 1 - активация еды слот 0");
        UseFood(0);
    }
    
    private void OnFood2(InputAction.CallbackContext ctx) 
    { 
        Debug.Log("🍽️ [Input] Нажата клавиша 2 - активация еды слот 1");
        UseFood(1);
    }
    
    private void OnFood3(InputAction.CallbackContext ctx) 
    { 
        Debug.Log("🍽️ [Input] Нажата клавиша 3 - активация еды слот 2");
        UseFood(2);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!_showAttackGizmos) return;
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
            
            if (attack is MainAttackLogic)
            {
                var data = attack.Data as MainAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Vector2 center = pos + face * radius * data.ForwardOffset;
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(center, radius);
                continue;
            }
            
            if (attack is DownAttackLogic)
            {
                var data = attack.Data as MainAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Vector2 center = pos + Vector2.down * radius * data.ForwardOffset;
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(center, radius);
                continue;
            }
            
            if (attack is ParryAttackLogic)
            {
                var data = attack.Data as ParryAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pos, radius);
            }
        }
    }
    
}
