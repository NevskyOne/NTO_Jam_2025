using UnityEngine;
using Core.Data.ScriptableObjects;
using System.Collections.Generic;
using Core.Interfaces;

namespace Abilities.Food
{
    // Рататуй — выпускает 3 зентаки(ввиде лиан) по направлению к курсору (2x урон по жиру)
    public class RatatouilleAbility : IAttack
    {
        public RatatouilleAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.1f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            if (direction == Vector2.zero) direction = Vector2.right;
            direction = direction.normalized;

            float step = Data.AttackRadius * 0.75f;
            float radius = Data.AttackRadius * 0.8f;
            Vector2 origin = Owner.position;
            List<IHittable> hit = new();

            for (int i = 1; i <= 3; i++)
            {
                Vector2 p = origin + direction * (step * i);
                Collider2D[] cols = Physics2D.OverlapCircleAll(p, radius);
                foreach (var c in cols)
                {
                    var h = c.GetComponent<IHittable>();
                    if (h != null && !hit.Contains(h))
                    {
                        hit.Add(h);
                        
                        float damage = GetDamage();
                        // 2x урон по жиру
                        if (HasGreaseEffect(c.gameObject))
                        {
                            damage *= 2f;
                            Debug.Log($"Ratatouille: bonus damage to greasy enemy {c.name}");
                        }
                        
                        h.TakeDamage(damage);
                        Debug.Log($"Ratatouille vine hit {c.name} for {damage} damage");
                    }
                }
            }
            Debug.Log($"Ratatouille: 3 hits along {direction}, dmg={GetDamage()}, step={step}, r={radius}");
        }

        private bool HasGreaseEffect(GameObject target)
        {
            // Проверяем наличие эффекта жирности
            return target.CompareTag("Greasy") || 
                   target.name.Contains("Grease") || 
                   target.name.Contains("Burger");
        }
    }
}
