using UnityEngine;
using Core.Data.ScriptableObjects;
using System.Collections.Generic;

public class SomeAttackLogic
{
    private readonly AttackDataSO _attackData;
    private List<IHittable> _hitObjects = new List<IHittable>();
    
    public SomeAttackLogic(AttackDataSO attackData)
    {
        _attackData = attackData ?? ScriptableObject.CreateInstance<AttackDataSO>();
    }
    
    public void PerformAttack(Vector2 direction, Vector3 position)
    {
        _hitObjects.Clear();
        Vector2 attackPoint = (Vector2)position + direction * _attackData.AttackRadius * 0.5f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint, _attackData.AttackRadius);
        
        foreach (var hitCollider in hitColliders)
        {
            IHittable hittable = hitCollider.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                hittable.TakeHit(_attackData.CalculateDamage(), direction * _attackData.KnockbackForce);
            }
        }
        
        Debug.Log($"Attack performed in direction {direction}, damage: {_attackData.CalculateDamage()}, radius: {_attackData.AttackRadius}");
    }
    
    public void PerformAirAttack(Vector2 direction, Vector3 position)
    {
        _hitObjects.Clear();
        float airAttackRadius = _attackData.AttackRadius * 1.2f;
        float airAttackDamage = _attackData.CalculateDamage() * 1.5f;
        Vector2 attackPoint = (Vector2)position + Vector2.down * airAttackRadius * 0.5f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint, airAttackRadius);
        
        foreach (var hitCollider in hitColliders)
        {
            IHittable hittable = hitCollider.GetComponent<IHittable>();
            if (hittable != null && !_hitObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
                hittable.TakeHit(airAttackDamage, Vector2.down * _attackData.KnockbackForce * 1.5f);
            }
        }
        
        Debug.Log($"Air attack performed, damage: {airAttackDamage}, radius: {airAttackRadius}");
    }
    
    public void PerformParry(Vector3 position)
    {
        float parryRadius = _attackData.AttackRadius * 1.5f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, parryRadius);
        
        foreach (var hitCollider in hitColliders)
        {
            // Здесь логика для отражения снарядов
            // Projectile projectile = hitCollider.GetComponent<Projectile>();
            // if (projectile != null) projectile.Reflect();
        }
        
        Debug.Log($"Parry performed, radius: {parryRadius}");
    }
}

// Интерфейс для объектов, которые могут получать урон
public interface IHittable
{
    void TakeHit(float damage, Vector2 knockback);
    void Stun(float duration);
}
