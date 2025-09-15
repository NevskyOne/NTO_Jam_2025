using System;
using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;
using Zenject;

[Serializable]
public class ParryAttackLogic : IAttack
{
    [SerializeField] private ParryAttackData _data;

    AttackDataSO IAttack.Data
    {
        get => _data;
        set => _data = (ParryAttackData)value;
    }
    
    private Transform _owner;
    private Player _player;
    private PlayerInput _input;
    private Coroutine _cooldownRoutine;

    [Inject]
    private void Construct(Player player, PlayerInput input)
    {
        _owner = player.transform;
        _player = player;
        _input = input;
    }

    public void Activate()
    {
        _input.actions[_data.InputBinding].performed += _ => PerformAttack(_player.Movement.LastDirection);
    }

    public void Deactivate()
    {
        _input.actions[_data.InputBinding].performed -= _ => PerformAttack(_player.Movement.LastDirection);
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null || _cooldownRoutine != null) return;
        // Логика парирования - создаем защитную область вокруг игрока
        float radius = _data.Radius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.position, radius);
        
        // Единичная визуализация парирования
        DrawDebugCircle(_owner.position, radius, Color.green, 0.5f);
        
        foreach (var hit in hits)
        {
            if (hit.transform == _owner) continue;
            
            // Здесь может быть логика отражения снарядов или оглушения врагов
            // debug log removed
        }
        
        _cooldownRoutine = _player.StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(_data.AttackCooldown);
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
