using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Чай — DoT по площади (урон каждый ход m ходов; 2x на лёд)
    public class TeaAbility : IAttack
    {
        public TeaAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.0f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            
            Vector2 point = Owner.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, Data.AttackRadius);
            
            foreach (var col in hits)
            {
                if (col.transform == Owner) continue;
                var h = col.GetComponent<IHittable>();
                if (h != null)
                {
                    float damage = GetDamage();
                    // 2x урон против льда
                    if (col.name.Contains("Ice") || col.name.Contains("Frost"))
                    {
                        damage *= 2f;
                    }
                    
                    h.TakeDamage(damage);
                    Debug.Log($"Tea hit {col.name} for {damage} damage");
                }
            }
            
            Debug.Log($"Tea attack used: area={Data.AttackRadius}");
            _attackDurationTimer = Data.AttackCooldown;
        }
    }
}
