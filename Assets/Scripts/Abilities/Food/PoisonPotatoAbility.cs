using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Ядовитая картошка — волна, отталкивание (TODO: волна по направлению движения, 1.2x урон по целям с любым эффектом)
    public class PoisonPotatoAbility : FoodAttackBase
    {
        public PoisonPotatoAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.2f;
    }
}
