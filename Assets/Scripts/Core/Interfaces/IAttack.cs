using UnityEngine;

namespace Core.Interfaces
{
    public interface IAttack
    {
        void PerformAttack(Vector2 direction);
        float GetDamage();
        float GetAttackRadius();
    }
}
