using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using Unity.VisualScripting;
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
    private MainUIInteractions _mainUI;
    private int _currentPills;

    private IInteractable _currentInteractable;
    public PlayerState State => _state;
    public CurrentPlayerData Data => _data;
    public Transform DialogueBubblePos => _dialogueBubblePos;
    public PlayerMovementLogic Movement => (PlayerMovementLogic)_movement;
    
    [Header("Debug/Visuals")]
    [SerializeField] private bool _showAttackGizmos = true;

    [Inject]
    private void Construct(PlayerDataSO playerData, MoveDataSO moveData, PlayerInput input, PlayerMovementLogic movement, DiContainer container, MainUIInteractions mainUI)
    {
        
        _playerData = playerData;
        _moveData = moveData;
        _playerInput = input; 
        _container = container;
        _mainUI = mainUI;
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
            _data.Health = _playerData.MaxHealth;
            _data.Money = _playerData.StartMoney;
            _data.Reputation = _playerData.StartReputation;
        }

        if (_mainAttackSet != null)
        {
            for (int i = 0; i < _mainAttackSet.Count; i++)
            {
                var a = _mainAttackSet[i];
                Debug.Log($"Attack[{i}]: {a?.GetType().Name} binding='{a?.Data?.InputBinding}'");
            }
        }
        
        _mainUI.UpdateText();
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
        if (grounded && _state == PlayerState.Jumping)
        {
            _state = PlayerState.Idle;
        }
        Debug.Log($"Ground state changed: {grounded}");
    }


    public void AddAbility(FoodUI foodUI)
    {

        if (_abilitiesSet[0] == null)
        {
            _abilitiesSet[0] = foodUI.Food;
            foodUI.Food.Activate();
            _data.UsedFood[0] = foodUI.Id;
        }
        else if (_abilitiesSet[1] == null)
        {
            _abilitiesSet[1] = foodUI.Food;
            foodUI.Food.Activate();
            _data.UsedFood[1] = foodUI.Id;
        }
        else if (_abilitiesSet[2] == null)
        {
            _abilitiesSet[2] = foodUI.Food;
            foodUI.Food.Activate();
            _data.UsedFood[2] = foodUI.Id;
        }
        else
        {
            AbilityToInventory(foodUI);
            foodUI.State = FoodState.Inventory;
            return;
        }
        foodUI.State = FoodState.Abilities;
    }
    
    public void SwitchAbility(FoodUI inventory, int slot)
    {
        var indexOfInvent = _data.InventoryFood.IndexOf(inventory.Id);
        
        (_data.UsedFood[slot], _data.InventoryFood[indexOfInvent]) = (inventory.Id, _data.UsedFood[slot]);
        _abilitiesSet[slot].Deactivate();
        _abilitiesSet[slot] = inventory.Food;
        _abilitiesSet[slot].Activate();
        inventory.State = FoodState.Abilities;
    }

    public void AbilityToMain(FoodUI inventory, int slot)
    {
        if (_abilitiesSet[slot] != null)
        {
            SwitchAbility(inventory, slot);
        }
        else
        {
            AddAbility(inventory);
        }
    }

    public void AbilityToInventory(FoodUI food)
    {
        _data.InventoryFood.Add(food.Id);
        food.Food.Deactivate();
    }
    
    public void SwitchAbility(int fromSlot, int toSlot)
    {
        (_abilitiesSet[fromSlot], _abilitiesSet[toSlot]) = (_abilitiesSet[toSlot], _abilitiesSet[fromSlot]);
        (_data.UsedFood[fromSlot], _data.UsedFood[toSlot]) = (_data.UsedFood[toSlot], _data.UsedFood[fromSlot]);
    }
    
    
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
        _mainUI.ChangeDeathState(true);
    }

    public void AddReputation()
    {
        _data.Reputation += 1;
        _mainUI.UpdateText();
    }
    
    public void AddMoney(int amount)
    {
        _data.Money += Mathf.Clamp(amount, 0, 999999999);
        _mainUI.UpdateText();
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
    
    private void OnFood1(InputAction.CallbackContext ctx) => UseFood(0);
    private void OnFood2(InputAction.CallbackContext ctx) => UseFood(1);
    private void OnFood3(InputAction.CallbackContext ctx) => UseFood(2);

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
                var data = attack.Data as Core.Data.ScriptableObjects.MainAttackData;
                if (data == null) continue;
                float radius = data.Radius;
                Vector2 center = pos + face * radius * data.ForwardOffset;
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(center, radius);
                continue;
            }
            
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