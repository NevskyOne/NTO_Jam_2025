using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

public class ParryAttackLogic : IAttack
{
    private readonly AttackDataSO _data;
    private readonly Transform _owner;
    private float _attackDurationTimer;

    public ParryAttackLogic(AttackDataSO data, Transform owner)
    {
        _data = data;
        _owner = owner;
    }

    public void PerformAttack(Vector2 direction)
    {
        // Логика парирования - создаем защитную область вокруг игрока
        float radius = _data.AttackRadius * _data.ParryRadiusMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.position, radius);
        
        // Единичная визуализация парирования
        DrawDebugCircle(_owner.position, radius, Color.green, 0.5f);
        
        foreach (var hit in hits)
        {
            if (hit.transform == _owner) continue;
            
            // Здесь может быть логика отражения снарядов или оглушения врагов
            Debug.Log($"Parry detected: {hit.name}");
        }

        Debug.Log($"Parry: r={_data.AttackRadius}");
        _attackDurationTimer = _data.ParryCooldown;
    }

    public float GetAttackRadius() => _data.AttackRadius * _data.ParryRadiusMultiplier;

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
