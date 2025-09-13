using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Пельмени — липкие ловушки (TODO: спавнить ловушку, станить врагов n сек)
    public class DumplingAbility : FoodAttackBase
    {
        public DumplingAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 0.6f;
    }
}
