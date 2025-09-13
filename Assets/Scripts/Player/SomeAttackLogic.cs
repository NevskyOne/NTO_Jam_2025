using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;

public class MainAttackLogic : IAttack
{
    private readonly AttackDataSO _data;
    private readonly Transform _owner;
    private readonly List<IHittable> _hitObjects = new();

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

        Vector2 point = (Vector2)_owner.position + direction.normalized * _data.AttackRadius * 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, _data.AttackRadius);

        foreach (var col in hits)
        {
            var hittable = col.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                hittable.TakeDamage(_data.BaseDamage);
            }
        }

        Debug.Log($"MainAttack: dir={direction}, dmg={_data.BaseDamage}, r={_data.AttackRadius}");
    }

    public float GetDamage() => _data.BaseDamage;
    public float GetAttackRadius() => _data.AttackRadius;
}

public class DownAttackLogic : IAttack
{
    private readonly AttackDataSO _data;
    private readonly Transform _owner;
    private readonly List<IHittable> _hitObjects = new();

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
        float radius = _data.AttackRadius * 1.2f;
        float dmg = _data.BaseDamage; // Можем масштабировать при необходимости

        Vector2 point = (Vector2)_owner.position + dir * radius * 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);

        foreach (var col in hits)
        {
            var hittable = col.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                hittable.TakeDamage(dmg);
            }
        }

        Debug.Log($"DownAttack: dmg={dmg}, r={radius}");
    }

    public float GetDamage() => _data.BaseDamage;
    public float GetAttackRadius() => _data.AttackRadius * 1.2f;
}

public class ParryAttackLogic : IAttack
{
    private readonly AttackDataSO _data;
    private readonly Transform _owner;

    public ParryAttackLogic(AttackDataSO data, Transform owner)
    {
        _data = data;
        _owner = owner;
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null) return;
        float radius = _data.AttackRadius * 1.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.position, radius);

        // Здесь может быть логика отражения снарядов/стан и т.п.
        Debug.Log($"Parry: r={radius}, hits={hits.Length}");
    }

    public float GetDamage() => 0f;
    public float GetAttackRadius() => _data.AttackRadius * 1.5f;
}
