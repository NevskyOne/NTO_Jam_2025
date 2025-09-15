using System;
using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Zenject;

[Serializable]
public class DownAttackLogic : IAttack
{
    [SerializeField] private MainAttackData _data;

    AttackDataSO IAttack.Data
    {
        get => _data;
        set => _data = (MainAttackData)value;
    }
    
    private Transform _owner;
    private readonly List<IHittable> _hitObjects = new();
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
        _hitObjects.Clear();

        Vector2 dir = Vector2.down;
        float radius = _data.Radius;
        int dmg = _data.BaseDamage;

        // Смещаем область атаки вниз от игрока
        Vector2 attackCenter = (Vector2)_owner.position + Vector2.down * radius * _data.ForwardOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, radius);

        // Визуализация области атаки для тестирования (рисуем окружность через линии)
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
            }
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
