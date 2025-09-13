using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;

namespace Abilities.Food
{
    public abstract class FoodAttackBase : IAttack
    {
        protected readonly AttackDataSO Data;
        protected readonly Transform Owner;

        protected FoodAttackBase(AttackDataSO data, Transform owner)
        {
            Data = data;
            Owner = owner;
        }

        protected virtual float DamageMultiplier => 1f;

        public virtual void PerformAttack(Vector2 direction)
        {
            if (Owner == null) return;
            Vector2 point = Owner.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, Data.AttackRadius);
            float dmg = GetDamage();
            foreach (var col in hits)
            {
                var h = col.GetComponent<IHittable>();
                if (h != null) h.TakeDamage(dmg);
            }
            Debug.Log($"{GetType().Name} used: dmg={dmg}, r={Data.AttackRadius}");
        }

        public virtual float GetDamage() => Data.BaseDamage * DamageMultiplier;
        public float GetAttackRadius() => Data.AttackRadius;
    }
}
