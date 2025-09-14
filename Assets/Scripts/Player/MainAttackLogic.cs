using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;

public class MainAttackLogic : IDamageAttack
{
    private readonly AttackDataSO _data;
    private readonly Transform _owner;
    private readonly List<IHittable> _hitObjects = new();
    private float _attackDurationTimer;

    public MainAttackLogic(AttackDataSO data, Transform owner)
    {
        _data = data;
        _owner = owner;
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null) return;
        _hitObjects.Clear();
        if (direction == Vector2.zero) direction = Vector2.right;

        // Атака направлена вперед от игрока
        float radius = _data.AttackRadius * _data.MainAttackRadiusMultiplier;
        Vector2 attackCenter = (Vector2)_owner.position + direction.normalized * radius * _data.MainAttackForwardOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, radius);

        // Единичная визуализация атаки
        DrawDebugCircle(attackCenter, radius, Color.red, 0.5f);

        foreach (var col in hits)
        {
            // Исключаем самого игрока из атаки
            if (col.transform == _owner) continue;

            var hittable = col.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                hittable.TakeDamage(_data.MainAttackDamage);
                Debug.Log($"MainAttack hit: {col.name} for {_data.MainAttackDamage} damage");
            }
        }

        Debug.Log($"MainAttack: dir={direction}, dmg={_data.MainAttackDamage}, r={radius}");
        _attackDurationTimer = _data.MainAttackCooldown;
    }

    public float GetDamage() => _data.BaseDamage;
    public float GetAttackRadius() => _data.AttackRadius;

    public float GetAttackDuration()
    {
        return _attackDurationTimer;
    }

    public void UpdateCooldown()
    {
        if (_attackDurationTimer > 0)
        {
            _attackDurationTimer -= Time.deltaTime;
        }
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
