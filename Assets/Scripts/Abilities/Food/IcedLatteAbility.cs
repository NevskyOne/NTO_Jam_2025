using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    // Айс латте — выпускает ледяные осколки в врагов в направлении курсора по клику, накладывает медлительность на врагов в течение n секунд (2x на огонь)
    public class IcedLatteAbility : IAttack
    {
        public IcedLatteAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 0.8f;

        public override void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            
            // Выпускаем 3 ледяных осколка в направлении
            for (int i = 0; i < 3; i++)
            {
                float angle = (i - 1) * 15f; // -15°, 0°, +15°
                Vector2 shardDirection = RotateVector(direction.normalized, angle);
                FireIceShard(shardDirection);
            }
            
            Debug.Log($"IcedLatte: fired 3 ice shards in direction {direction}");
            _attackDurationTimer = Data.AttackCooldown;
        }

        private void FireIceShard(Vector2 direction)
        {
            // Проверяем попадание по линии
            RaycastHit2D hit = Physics2D.Raycast(Owner.position, direction, Data.AttackRadius * 2f);
            
            if (hit.collider != null && hit.collider.transform != Owner)
            {
                var hittable = hit.collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    float damage = GetDamage();
                    // 2x урон против огня
                    if (hit.collider.name.Contains("Fire") || hit.collider.name.Contains("Flame"))
                    {
                        damage *= 2f;
                    }
                    
                    hittable.TakeDamage(damage);
                    Debug.Log($"Ice shard hit {hit.collider.name} for {damage} damage + slow");
                }
            }
            
            // Визуализация осколка
            Debug.DrawRay(Owner.position, direction * Data.AttackRadius * 2f, Color.cyan, 0.5f);
        }

        private Vector2 RotateVector(Vector2 vector, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRadians);
            float sin = Mathf.Sin(angleRadians);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }
    }
}
