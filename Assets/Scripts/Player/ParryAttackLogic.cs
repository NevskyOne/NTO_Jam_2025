using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;
using Zenject;

[System.Serializable]
public class ParryAttackLogic : IAttack
{
    [field: SerializeField] AttackDataSO IAttack.Data { get; set; }
    private ParryAttackData Data => (ParryAttackData)((IAttack)this).Data;
    
    private Transform _owner;
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
        string bindingName = (baseData != null && !string.IsNullOrEmpty(baseData.InputBinding)) ? baseData.InputBinding : "RightMouse";
        var action = asset.FindAction(bindingName, false)
                     ?? asset.FindAction($"Player/{bindingName}", false)
                     ?? asset.FindAction($"Actions/{bindingName}", false)
                     ?? asset.FindAction($"UI/{bindingName}", false);
        if (action == null)
        {
            Debug.LogError($"[{GetType().Name}] Input action not found. Tried '{bindingName}' in maps [default, Player, Actions, UI]");
            return;
        }
        _onPerformed = ctx => PerformAttack(_player != null ? _player.Movement.LastDirection : Vector2.right);
        action.performed += _onPerformed;
        Debug.Log($"[{GetType().Name}] Subscribed to action '{action.name}' (map: {action.actionMap?.name})");
    }

    public void Deactivate()
    {
        if (_input == null) return;
        var asset = _input.actions;
        if (asset == null) return;
        var baseData = ((IAttack)this).Data;
        string bindingName = (baseData != null && !string.IsNullOrEmpty(baseData.InputBinding)) ? baseData.InputBinding : "RightMouse";
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
        var data = baseData as ParryAttackData;
        if (data == null)
        {
            Debug.LogError($"[{GetType().Name}] Wrong Data type. Expected ParryAttackData, got {baseData.GetType().Name}.");
            return;
        }
        // Логика парирования - создаем защитную область вокруг игрока
        float radius = data.Radius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.position, radius);
        
        // Единичная визуализация парирования
        DrawDebugCircle(_owner.position, radius, Color.green, 0.5f);
        
        foreach (var hit in hits)
        {
            if (hit.transform == _owner) continue;
            
            // Здесь может быть логика отражения снарядов или оглушения врагов
            Debug.Log($"Parry detected: {hit.name}");
        }
        
        if (_player != null)
            _cooldownRoutine = _player.StartCoroutine(CooldownRoutine(data.AttackCooldown));
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
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
