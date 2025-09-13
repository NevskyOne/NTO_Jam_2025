using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Айс латте — выпускает ледяные осколки + накладывает замедление (TODO: реализовать осколки и slow; 2x на огонь)
    public class IcedLatteAbility : FoodAttackBase
    {
        public IcedLatteAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 0.8f;
    }
}
