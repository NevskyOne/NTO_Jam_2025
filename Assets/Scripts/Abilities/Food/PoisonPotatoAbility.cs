using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Ядовитая картошка — посылает волну картошки, которая отталкивает врагов и наносит урон + яд
    public class PoisonPotatoAbility : FoodAttackBase
    {
        public PoisonPotatoAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.2f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            if (direction == Vector2.zero) direction = Vector2.right;
            
            direction = direction.normalized;
            
            // Создаем волну картошки в направлении
            CreatePotatoWave(direction);
            
            Debug.Log($"Poison Potato: wave sent in direction {direction}");
            _attackDurationTimer = Data.AttackCooldown;
        }

        private void CreatePotatoWave(Vector2 direction)
        {
            Vector2 startPos = Owner.position;
            float waveLength = Data.AttackRadius * 2f;
            int segments = 5; // 5 сегментов волны
            
            for (int i = 1; i <= segments; i++)
            {
                Vector2 segmentPos = startPos + direction * (waveLength / segments * i);
                float segmentRadius = Data.AttackRadius * 0.8f;
                
                // Проверяем врагов в каждом сегменте
                Collider2D[] hits = Physics2D.OverlapCircleAll(segmentPos, segmentRadius);
                
                foreach (var col in hits)
                {
                    if (col.transform == Owner) continue;
                    
                    var hittable = col.GetComponent<IHittable>();
                    if (hittable != null)
                    {
                        float damage = GetDamage();
                        
                        // Бонус урон против врагов с эффектами
                        if (HasStatusEffects(col.gameObject))
                        {
                            damage *= 1.2f;
                        }
                        
                        hittable.TakeDamage(damage);
                        
                        // Отталкиваем врага
                        var rb = col.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.AddForce(direction * 8f, ForceMode2D.Impulse);
                        }
                        
                        // Добавляем тег яда
                        if (!col.CompareTag("Poisoned"))
                        {
                            col.tag = "Poisoned";
                        }
                        
                        Debug.Log($"Poison wave hit {col.name} for {damage} damage + poison + knockback");
                    }
                }
                
                // Визуализация сегмента волны
                DrawWaveSegment(segmentPos, segmentRadius, i);
            }
        }

        private bool HasStatusEffects(GameObject target)
        {
            return target.CompareTag("Poisoned") || 
                   target.CompareTag("Greasy") || 
                   target.CompareTag("Stunned") ||
                   target.name.Contains("Effect");
        }

        private void DrawWaveSegment(Vector2 center, float radius, int segmentIndex)
        {
            Color waveColor = Color.Lerp(Color.green, Color.yellow, segmentIndex / 5f);
            
            int points = 12;
            float angle = 2 * Mathf.PI / points;
            
            for (int i = 0; i < points; i++)
            {
                float currentAngle = i * angle;
                float nextAngle = (i + 1) * angle;
                
                Vector2 currentPoint = center + new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                Vector2 nextPoint = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
                
                Debug.DrawLine(currentPoint, nextPoint, waveColor, 0.8f);
            }
        }
    }
}
