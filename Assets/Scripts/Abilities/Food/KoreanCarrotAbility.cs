using UnityEngine;
using Core.Data.ScriptableObjects;

namespace Abilities.Food
{
    // Корейская морковка — дабл-джамп (TODO: добавить стан после удара в падении)
    public class KoreanCarrotAbility : FoodAttackBase
    {
        public KoreanCarrotAbility(AttackDataSO data, Transform owner) : base(data, owner) { }
        protected override float DamageMultiplier => 0.0f; // не атакующая способность

        public override void PerformAttack(Vector2 direction)
        {
            // Активируем пассив: +1 прыжок
            var player = Owner != null ? Owner.GetComponent<Player>() : null;
            if (player != null)
            {
                player.SetExtraJumps(1);
                UnityEngine.Debug.Log("KoreanCarrot: extra jump enabled (+1)");
            }
        }

        public override float GetDamage() => 0f; // не наносит урон
    }
}
