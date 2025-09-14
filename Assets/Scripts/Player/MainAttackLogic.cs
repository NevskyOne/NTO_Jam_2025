using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Zenject;

public class MainAttackLogic : IAttack
{
    [field: SerializeReference] AttackDataSO IAttack.Data { get; set; }
    private MainAttackData Data => (MainAttackData)((IAttack)this).Data;
    
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
        _input.actions[Data.InputBinding].performed += _ => PerformAttack(_player.Movement.LastDirection);
    }

    public void Deactivate()
    {
        _input.actions[Data.InputBinding].performed -= _ => PerformAttack(_player.Movement.LastDirection);
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null || _cooldownRoutine != null) return;
        
        _hitObjects.Clear();
        if (direction == Vector2.zero) direction = Vector2.right;

        // Атака направлена вперед от игрока
        float radius = Data.Radius;
        Vector2 attackCenter = (Vector2)_owner.position + direction.normalized * radius * Data.ForwardOffset;
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
                hittable.TakeDamage(Data.BaseDamage);
                Debug.Log($"MainAttack hit: {col.name} for {Data.BaseDamage} damage");
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
