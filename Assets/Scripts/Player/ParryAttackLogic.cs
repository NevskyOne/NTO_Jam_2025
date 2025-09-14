using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;
using Zenject;

public class ParryAttackLogic : IAttack
{
    [field: SerializeReference] AttackDataSO IAttack.Data { get; set; }
    private ParryAttackData Data => (ParryAttackData)((IAttack)this).Data;
    
    private Transform _owner;
    private Player _player;
    private Coroutine _cooldownRoutine;
    
    [Inject]
    private void Construct(Player player)
    {
        _owner = player.transform;
        _player = player;
    }

    public void Activate()
    {
    }

    public void Deactivate()
    {
    }

    public void PerformAttack(Vector2 direction)
    {
        if (_owner == null || _cooldownRoutine != null) return;
        // Логика парирования - создаем защитную область вокруг игрока
        float radius = Data.Radius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.position, radius);
        
        // Единичная визуализация парирования
        DrawDebugCircle(_owner.position, radius, Color.green, 0.5f);
        
        foreach (var hit in hits)
        {
            if (hit.transform == _owner) continue;
            
            // Здесь может быть логика отражения снарядов или оглушения врагов
            Debug.Log($"Parry detected: {hit.name}");
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
