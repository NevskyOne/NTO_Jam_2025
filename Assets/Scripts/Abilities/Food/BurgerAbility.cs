using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Бургер — накладывает 'жирность' на врага (TODO: добавить DoT и эффект ускорения врага)
    public class BurgerAbility : FoodAttackBase
    {
        public BurgerAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.0f;
    }
}
