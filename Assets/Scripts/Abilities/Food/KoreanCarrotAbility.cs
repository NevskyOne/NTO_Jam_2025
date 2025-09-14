using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Корейская морковка — дает дабл-джамп + урон при падении
    public class KoreanCarrotAbility : IAttack
    {
        private bool _extraJumpUsed = false;
        private bool _wasFalling = false;
        private float _fallStartHeight = 0f;
        
        public KoreanCarrotAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.0f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            
            var rb = Owner.GetComponent<Rigidbody2D>();
            if (rb == null) return;
            
            // Даем дополнительный прыжок
            if (!_extraJumpUsed && rb.linearVelocity.y <= 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f); // Сила прыжка
                _extraJumpUsed = true;
                Debug.Log("Korean Carrot: extra jump used");
            }
            
            _attackDurationTimer = Data.AttackCooldown;
        }

        public void Update()
        {
            if (Owner == null) return;
            
            var rb = Owner.GetComponent<Rigidbody2D>();
            if (rb == null) return;
            
            // Отслеживаем падение
            if (rb.linearVelocity.y < -2f && !_wasFalling)
            {
                _wasFalling = true;
                _fallStartHeight = Owner.position.y;
            }
            
            // Проверяем приземление
            if (_wasFalling && rb.linearVelocity.y >= 0)
            {
                float fallDistance = _fallStartHeight - Owner.position.y;
                if (fallDistance > 2f) // Минимальная высота падения
                {
                    PerformLandingAttack(fallDistance);
                }
                
                _wasFalling = false;
                _extraJumpUsed = false; // Сброс дополнительного прыжка при приземлении
            }
        }

        private void PerformLandingAttack(float fallDistance)
        {
            float landingRadius = Data.AttackRadius * 1.2f;
            float bonusDamage = fallDistance * 0.5f; // Бонус урон от высоты падения
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(Owner.position, landingRadius);
            
            foreach (var col in hits)
            {
                if (col.transform == Owner) continue;
                
                var hittable = col.GetComponent<IHittable>();
                if (hittable != null)
                {
                    float totalDamage = GetDamage() + bonusDamage;
                    hittable.TakeDamage(totalDamage);
                    
                    // Оглушаем врага
                    var rb = col.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    
                    Debug.Log($"Korean Carrot landing hit {col.name} for {totalDamage} damage + stun");
                }
            }
            
            // Визуализация удара при приземлении
            DrawLandingEffect(Owner.position, landingRadius);
        }

        private void DrawLandingEffect(Vector2 center, float radius)
        {
            // Рисуем эффект удара при приземлении
            int rays = 8;
            for (int i = 0; i < rays; i++)
            {
                float angle = (2 * Mathf.PI / rays) * i;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Debug.DrawRay(center, direction * radius, Color.orange, 1f);
            }
            
            // Круг удара
            int segments = 16;
            float segmentAngle = 2 * Mathf.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                float currentAngle = i * segmentAngle;
                float nextAngle = (i + 1) * segmentAngle;
                
                Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
                
                Debug.DrawLine(currentPoint, nextPoint, Color.red, 1f);
            }
        }
    }
}
