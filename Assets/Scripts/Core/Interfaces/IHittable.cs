using UnityEngine;

namespace Core.Interfaces
{
    public interface IHittable
    {
        void TakeDamage(float amount);
        bool IsAlive();
        void Die();
        float GetCurrentHealth();
        float GetMaxHealth();
    }
}
