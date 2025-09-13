using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Драконий фрукт — таран с шипами (TODO: реализовать щит-коллайдер и таран)
    public class DragonFruitAbility : FoodAttackBase
    {
        public DragonFruitAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.2f;
    }
}
