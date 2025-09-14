using UnityEngine;

namespace Core.Interfaces
{
    public interface IAttack
    {
        void PerformAttack(Vector2 direction);
        float GetAttackRadius();
        float GetAttackDuration();
    }
    
    public interface IDamageAttack : IAttack
    {
        float GetDamage();
    }
}
