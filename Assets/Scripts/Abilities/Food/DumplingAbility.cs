using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Пельмени — возможность ставить под собой липкие ловушки-тесто (станит врагов, n сек)
    public class DumplingAbility : FoodAttackBase
    {
        public DumplingAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 0.5f; // Низкий урон, основной эффект - стан

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            
            // Ставим липкую ловушку под игроком
            Vector2 trapPosition = Owner.position;
            
            // Проверяем врагов в радиусе ловушки
            Collider2D[] hits = Physics2D.OverlapCircleAll(trapPosition, Data.AttackRadius);
            
            foreach (var col in hits)
            {
                if (col.transform == Owner) continue;
                
                var hittable = col.GetComponent<IHittable>();
                if (hittable != null)
                {
                    // Наносим урон и применяем стан
                    hittable.TakeDamage(GetDamage());
                    
                    // Останавливаем врага
                    var rb = col.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    
                    Debug.Log($"Dumpling trap caught {col.name} - damage + stun");
                }
            }
            
            // Визуализация ловушки
            DrawTrapEffect(trapPosition, Data.AttackRadius);
            
            Debug.Log($"Dumpling: sticky trap placed at {trapPosition}");
            _attackDurationTimer = Data.AttackCooldown;
        }

        private void DrawTrapEffect(Vector2 center, float radius)
        {
            // Рисуем липкую ловушку
            int segments = 12;
            float angle = 2 * Mathf.PI / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float currentAngle = i * angle;
                float nextAngle = (i + 1) * angle;
                
                Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
                
                Debug.DrawLine(currentPoint, nextPoint, Color.yellow, 0.3f);
            }
            
            // Крест в центре
            Debug.DrawLine(center + Vector2.left * radius * 0.5f, center + Vector2.right * radius * 0.5f, Color.yellow, 0.3f);
            Debug.DrawLine(center + Vector2.up * radius * 0.5f, center + Vector2.down * radius * 0.5f, Color.yellow, 0.3f);
        }
    }
}
