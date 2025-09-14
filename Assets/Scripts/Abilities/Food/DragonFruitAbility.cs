using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Драконий фрукт — таранящий щит с шипами
    public class DragonFruitAbility : IAttack
    {
        private bool _shieldActive = false;
        
        public DragonFruitAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.5f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null || _shieldActive) return;
            
            _shieldActive = true;
            
            // Создаем защитный щит вокруг игрока
            Vector2 shieldCenter = Owner.position;
            float shieldRadius = Data.AttackRadius * 1.2f;
            
            // Проверяем врагов в радиусе щита
            Collider2D[] hits = Physics2D.OverlapCircleAll(shieldCenter, shieldRadius);
            
            foreach (var col in hits)
            {
                if (col.transform == Owner) continue;
                
                var hittable = col.GetComponent<IHittable>();
                if (hittable != null)
                {
                    // Наносим урон шипами щита
                    hittable.TakeDamage(GetDamage());
                    
                    // Отталкиваем врага от игрока
                    Vector2 knockbackDirection = (col.transform.position - Owner.position).normalized;
                    var rb = col.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                    }
                    
                    Debug.Log($"Dragon Fruit shield hit {col.name} for {GetDamage()} damage + knockback");
                }
            }
            
            // Визуализация щита
            DrawShieldEffect(shieldCenter, shieldRadius);
            
            Debug.Log($"Dragon Fruit: shield activated with radius {shieldRadius}");
            _attackDurationTimer = Data.AttackCooldown;
            _shieldActive = false;
        }

        private void DrawShieldEffect(Vector2 center, float radius)
        {
            // Рисуем щит с шипами
            int segments = 16;
            float angle = 2 * Mathf.PI / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float currentAngle = i * angle;
                float nextAngle = (i + 1) * angle;
                
                Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
                
                // Основной щит
                Debug.DrawLine(currentPoint, nextPoint, Color.red, 2f);
                
                // Шипы
                if (i % 2 == 0)
                {
                    Vector2 spikeEnd = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * (radius + 0.3f);
                    Debug.DrawLine(currentPoint, spikeEnd, Color.darkRed, 2f);
                }
            }
        }
    }
}
