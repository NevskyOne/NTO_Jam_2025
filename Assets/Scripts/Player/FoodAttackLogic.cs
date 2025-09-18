using System;
using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Zenject;

[Serializable]
public class FoodAttackLogic : IAttack
{
    [SerializeField] private FoodData _data;

    AttackDataSO IAttack.Data
    {
        get => _data;
        set => _data = (FoodData)value;
    }
    
    private Transform _owner;
    private readonly List<IHittable> _hitObjects = new();
    private Player _player;
    private PlayerInput _input;
    private Coroutine _cooldownCoroutine;
    private InputAction _cachedAction;
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
        if (_player == null || _input == null || _owner == null)
        {
            Debug.LogError($"[{GetType().Name}] Missing DI references (Player/Input/Owner)");
            return;
        }
        if (_input.actions == null)
        {
            Debug.LogError($"[{GetType().Name}] PlayerInput.actions is null (no InputActionAsset assigned)");
            return;
        }
        if (_data == null)
        {
            Debug.LogError($"[{GetType().Name}] Data is null. Assign FoodData in PlayerDataSO.AttackSet.");
            return;
        }
        
        string bindingName = string.IsNullOrEmpty(_data.InputBinding) ? "RightMouse" : _data.InputBinding;
        var asset = _input.actions;
        var action = asset.FindAction(bindingName, false)
                     ?? asset.FindAction($"Player/{bindingName}", false)
                     ?? asset.FindAction($"Actions/{bindingName}", false)
                     ?? asset.FindAction($"UI/{bindingName}", false);
        if (action == null)
        {
            Debug.LogError($"[{GetType().Name}] Input action not found. Tried '{bindingName}' in maps [default, Player, Actions, UI]");
            return;
        }
        if (_onPerformed == null)
            _onPerformed = ctx => PerformAttack(_player != null ? _player.Movement.LastDirection : Vector2.right);
        action.performed += _onPerformed;
        _cachedAction = action;
    }

    public void Deactivate()
    {
        if (_cachedAction != null && _onPerformed != null)
        {
            _cachedAction.performed -= _onPerformed;
        }
        _cachedAction = null;
        _onPerformed = null;
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null || _cooldownCoroutine != null) return;
        if (_data == null)
        {
            Debug.LogError($"[{GetType().Name}] Data is null. Assign FoodData in PlayerDataSO.AttackSet.");
            return;
        }
        
        // Применяем эффекты на себя (игрока)
        ApplyEffectsOnSelf();
        
        // Атакуем врагов и применяем эффекты на них
        AttackEnemies(direction);
        
        _cooldownCoroutine = _player.StartCoroutine(CooldownRoutine());
    }
    
    private void ApplyEffectsOnSelf()
    {
        if (_data.ApplyOnSelf == null) return;
        
        foreach (var effect in _data.ApplyOnSelf)
        {
            if (effect != null)
            {
                Debug.Log($"Применяем эффект на игрока: {effect.name}");
                effect.ApplyEffect(_player.gameObject);
            }
        }
    }
    
    private void AttackEnemies(Vector2 direction)
    {
        _hitObjects.Clear();
        if (direction == Vector2.zero) direction = Vector2.right;
        
        float radius = _data.Radius;
        Vector2 attackCenter = (Vector2)_owner.position + direction.normalized * radius * _data.ForwardOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, radius);
        
        DrawDebugCircle(attackCenter, radius, Color.green, 0.5f);

        foreach (var col in hits)
        {
            if (col.transform == _owner) continue;

            var hittable = col.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                
                // Наносим урон
                hittable.TakeDamage(_data.BaseDamage);
                
                // Применяем эффекты на цель
                ApplyEffectsOnTarget(col.gameObject);
            }
        }
    }
    
    private void ApplyEffectsOnTarget(GameObject target)
    {
        if (_data.ApplyOnTargets == null) return;
        
        foreach (var effect in _data.ApplyOnTargets)
        {
            if (effect != null)
            {
                Debug.Log($"Применяем эффект на цель: {effect.name}");
                effect.ApplyEffect(target);
            }
        }
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(_data.AttackCooldown);
        _cooldownCoroutine = null;
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
