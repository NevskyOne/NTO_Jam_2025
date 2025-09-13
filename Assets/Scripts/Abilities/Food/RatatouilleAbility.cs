using UnityEngine;
using Core.Data.ScriptableObjects;
using System.Collections.Generic;
using Core.Interfaces;

namespace Abilities.Food
{
    // Рататуй — выпускает 3 "тентакля" по направлению (TODO: заменить на реальные спавны лиан; 2x урон по "жиру")
    public class RatatouilleAbility : FoodAttackBase
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
                        h.TakeDamage(GetDamage());
                    }
                }
            }
            Debug.Log($"Ratatouille: 3 hits along {direction}, dmg={GetDamage()}, step={step}, r={radius}");
        }
    }
}
