using UnityEngine;

namespace Core.Interfaces
{
    public interface IEnemyMovable
    {
        void SetTarget(Transform target);
        void ClearTarget();
        void EnableMovement(bool enabled);
    }
}
