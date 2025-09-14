using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Бургер — накладывает на врагов "жирность" (ускоряет врага, но наносит DoT)
    public class BurgerAbility : FoodAttackBase
    {
        public BurgerAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.0f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            
            Vector2 attackPoint = (Vector2)Owner.position + direction.normalized * Data.AttackRadius * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, Data.AttackRadius);
            
            foreach (var col in hits)
            {
                if (col.transform == Owner) continue;
                
                var hittable = col.GetComponent<IHittable>();
                if (hittable != null)
                {
                    // Наносим урон
                    hittable.TakeDamage(GetDamage());
                    
                    // Применяем эффект жирности
                    ApplyGreaseEffect(col.gameObject);
                    
                    Debug.Log($"Burger hit {col.name} for {GetDamage()} damage + grease effect");
                }
            }
            
            Debug.Log($"Burger attack used at {attackPoint}");
            _attackDurationTimer = Data.AttackCooldown;
        }

        public void ApplyGreaseEffect(GameObject target)
        {
            // Добавляем тег жирности для других способностей
            if (!target.CompareTag("Greasy"))
            {
                target.tag = "Greasy";
            }
            
            // Ускоряем врага
            var rb = target.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(rb.linearVelocity.normalized * 3f, ForceMode2D.Impulse);
            }
            
            Debug.Log($"Grease effect applied to {target.name} - speed boost + DoT tag");
        }
        
        // Дополнительный метод для парирования
        public void ApplyGreasinessOnParry(GameObject target)
        {
            ApplyGreaseEffect(target);
            Debug.Log($"Burger parry effect: greasiness applied to {target.name}");
        }
    }
}
