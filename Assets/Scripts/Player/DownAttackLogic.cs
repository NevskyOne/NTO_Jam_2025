using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;

public class DownAttackLogic : IDamageAttack
{
    private readonly AttackDataSO _data;
    private readonly Transform _owner;
    private readonly List<IHittable> _hitObjects = new List<IHittable>();
    private float _attackDurationTimer;

    public DownAttackLogic(AttackDataSO data, Transform owner)
    {
        _data = data;
        _owner = owner;
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null) return;
        _hitObjects.Clear();

        Vector2 dir = Vector2.down;
        float radius = _data.AttackRadius * _data.DownAttackRadiusMultiplier;
        float dmg = _data.DownAttackDamage;

        // Смещаем область атаки вниз от игрока
        Vector2 attackCenter = (Vector2)_owner.position + Vector2.down * radius * _data.DownAttackDownOffset;
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

        Debug.Log($"DownAttack: dmg={dmg}, r={radius}");
        _attackDurationTimer = _data.DownAttackCooldown;
    }

    public float GetDamage() => _data.DownAttackDamage;
    public float GetAttackRadius() => _data.AttackRadius * _data.DownAttackRadiusMultiplier;

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
