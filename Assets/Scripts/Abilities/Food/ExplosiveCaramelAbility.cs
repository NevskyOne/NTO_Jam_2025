using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Взрывная карамель — ставит бомбу, которая взрывается через n секунд
    public class ExplosiveCaramelAbility : IAttack
    {
        public ExplosiveCaramelAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.3f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            
            // Размещаем бомбу в направлении атаки
            Vector2 bombPosition = (Vector2)Owner.position + direction.normalized * Data.AttackRadius * 0.7f;
            
            // Немедленный взрыв для упрощения
            ExplodeBomb(bombPosition);
            
            Debug.Log($"Explosive Caramel: bomb placed and exploded at {bombPosition}");
            _attackDurationTimer = Data.AttackCooldown;
        }

        private void ExplodeBomb(Vector2 position)
        {
            float explosionRadius = Data.AttackRadius * 1.5f;
            
            // Находим всех врагов в радиусе взрыва
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, explosionRadius);
            
            foreach (var col in hits)
            {
                if (col.transform == Owner) continue;
                
                var hittable = col.GetComponent<IHittable>();
                if (hittable != null)
                {
                    // Рассчитываем урон в зависимости от расстояния
                    float distance = Vector2.Distance(position, col.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius) * 0.5f; // От 100% до 50% урона
                    float damage = GetDamage() * damageMultiplier;
                    
                    hittable.TakeDamage(damage);
                    
                    // Отбрасываем врага от центра взрыва
                    Vector2 knockbackDirection = (col.transform.position - (Vector3)position).normalized;
                    var rb = col.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        float knockbackForce = 15f * damageMultiplier;
                        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                    }
                    
                    Debug.Log($"Explosion hit {col.name} for {damage} damage + knockback");
                }
            }
            
            // Визуализация взрыва
            DrawExplosionEffect(position, explosionRadius);
        }

        private void DrawExplosionEffect(Vector2 center, float radius)
        {
            // Рисуем взрыв
            int rays = 16;
            for (int i = 0; i < rays; i++)
            {
                float angle = (2 * Mathf.PI / rays) * i;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Debug.DrawRay(center, direction * radius, Color.red, 1f);
            }
            
            // Круги взрыва
            int segments = 20;
            float segmentAngle = 2 * Mathf.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                float currentAngle = i * segmentAngle;
                float nextAngle = (i + 1) * segmentAngle;
                
                Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
                
                Debug.DrawLine(currentPoint, nextPoint, Color.orange, 1f);
            }
        }
    }
}
