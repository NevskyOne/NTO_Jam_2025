using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Чай — DoT по площади (TODO: заменить мгновенный урон на периодический n урона m ходов; 2x на лёд)
    public class TeaAbility : FoodAttackBase
    {
        public TeaAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.0f;
    }
}
