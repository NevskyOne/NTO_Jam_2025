using System.Collections;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine;

public class BasicEnemyAttackLogic : MonoBehaviour, IAttack
{
    [SerializeField] private BasicEnemyAttackDataSO _data;
    
    private Coroutine _cooldownCoroutine;
    
    public AttackDataSO Data
    {
        get => _data;
        set => _data = (BasicEnemyAttackDataSO)value;
    }
    
    public BasicEnemyAttackDataSO.AttackMode Mode => _data?.Mode ?? BasicEnemyAttackDataSO.AttackMode.Melee;
    
    public void Activate()
    {
    }
    
    public void Deactivate()
    {
    }
    
    public void PerformAttack(Vector2 direction)
    {
        if (_cooldownCoroutine != null || _data == null) return;
        
        Vector2 attackPosition = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, _data.Radius);
        
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            
            var hittable = hit.GetComponent<IHittable>();
            if (hittable != null)
            {
                hittable.TakeDamage(_data.BaseDamage);
            }
        }
        
        _cooldownCoroutine = StartCoroutine(CooldownRoutine());
    }
    
    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(_data.AttackCooldown);
        _cooldownCoroutine = null;
    }
}
