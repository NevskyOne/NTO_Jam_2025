using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Взрывная карамель — установка бомбы с задержкой (TODO: спавн бомбы, задержка t, взрыв радиуса n)
    public class ExplosiveCaramelAbility : FoodAttackBase
    {
        public ExplosiveCaramelAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 1.5f;
    }
}
