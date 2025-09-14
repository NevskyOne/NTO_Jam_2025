using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Zenject;

[System.Serializable]
public class DownAttackLogic : IAttack
{
    [field: SerializeField] AttackDataSO IAttack.Data { get; set; }
    private MainAttackData Data => (MainAttackData)((IAttack)this).Data;
    
    private Transform _owner;
    private readonly List<IHittable> _hitObjects = new();
    private Player _player;
    private PlayerInput _input;
    private Coroutine _cooldownRoutine;
    private System.Action<InputAction.CallbackContext> _onPerformed;
    
    [Inject]
    private void Construct(Player player, PlayerInput input)
    {
        _owner = player.transform;
        _player = player;
        _input = input;
    }

    public void Activate()
    {
        // Rely on DI
        if (_player == null || _input == null || _owner == null)
        {
            Debug.LogError($"[{GetType().Name}] Missing DI references (Player/Input/Owner)");
            return;
        }
        var asset = _input.actions;
        if (asset == null)
        {
            Debug.LogError($"[{GetType().Name}] PlayerInput.actions is null (no InputActionAsset assigned)");
            return;
        }
        var baseData = ((IAttack)this).Data;
        string bindingName = (baseData != null && !string.IsNullOrEmpty(baseData.InputBinding)) ? baseData.InputBinding : "DownAttack";
        var action = asset.FindAction(bindingName, false)
                     ?? asset.FindAction($"Player/{bindingName}", false)
                     ?? asset.FindAction($"Actions/{bindingName}", false)
                     ?? asset.FindAction($"UI/{bindingName}", false);
        if (action == null)
        {
            Debug.LogError($"[{GetType().Name}] Input action not found. Tried '{bindingName}' in maps [default, Player, Actions, UI]");
            return;
        }
        _onPerformed = ctx => PerformAttack(_player != null ? _player.Movement.LastDirection : Vector2.down);
        action.performed += _onPerformed;
        Debug.Log($"[{GetType().Name}] Subscribed to action '{action.name}' (map: {action.actionMap?.name})");
    }

    public void Deactivate()
    {
        if (_input == null) return;
        var asset = _input.actions;
        if (asset == null) return;
        var baseData = ((IAttack)this).Data;
        string bindingName = (baseData != null && !string.IsNullOrEmpty(baseData.InputBinding)) ? baseData.InputBinding : "DownAttack";
        var action = asset.FindAction(bindingName, false)
                     ?? asset.FindAction($"Player/{bindingName}", false)
                     ?? asset.FindAction($"Actions/{bindingName}", false)
                     ?? asset.FindAction($"UI/{bindingName}", false);
        if (action != null && _onPerformed != null)
        {
            action.performed -= _onPerformed;
            _onPerformed = null;
            Debug.Log($"[{GetType().Name}] Unsubscribed from action '{action.name}' (map: {action.actionMap?.name})");
        }
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null || _cooldownRoutine != null) return;
        var baseData = ((IAttack)this).Data;
        if (baseData == null)
        {
            Debug.LogError($"[{GetType().Name}] Data is null. Assign AttackDataSO in PlayerDataSO.AttackSet.");
            return;
        }
        if (Data == null)
        {
            Debug.LogError($"[{GetType().Name}] Wrong Data type. Expected MainAttackData, got {baseData.GetType().Name}.");
            return;
        }
        _hitObjects.Clear();

        Vector2 dir = Vector2.down;
        float radius = Data.Radius;
        int dmg = Data.BaseDamage;

        // Смещаем область атаки вниз от игрока
        Vector2 attackCenter = (Vector2)_owner.position + Vector2.down * radius * Data.ForwardOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, radius);

        // Визуализация области атаки для тестирования (рисуем окружность через линии)
        Debug.Log($"Drawing DownAttack circle at {attackCenter} with radius {radius}");
        DrawDebugCircle(attackCenter, radius, Color.blue, 1f);

        foreach (var col in hits)
        {
            // Исключаем самого игрока из атаки
            if (col.transform == _owner) continue;
            
            // Проверяем что цель находится ниже игрока
            if (col.transform.position.y > _owner.position.y) continue;
            
            var hittable = col.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                hittable.TakeDamage(dmg);
                Debug.Log($"DownAttack hit: {col.name} for {dmg} damage");
            }
        }

        _cooldownRoutine = _player.StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(Data.AttackCooldown);
        _cooldownRoutine = null;
    }

    private void DrawDebugCircle(Vector2 center, float radius, Color color, float duration)
    {
        int segments = 32;
        float angle = 2 * Mathf.PI / segments;
        Vector2 previousPoint = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = i * angle;
            Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
            Debug.DrawLine(previousPoint, currentPoint, color, duration);
            previousPoint = currentPoint;
        }
    }
}
