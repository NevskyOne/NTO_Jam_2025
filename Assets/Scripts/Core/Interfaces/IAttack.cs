using Core.Data.ScriptableObjects;
using UnityEngine;

namespace Core.Interfaces
{

    public interface IAttack
    {
        [field: SerializeReference] public AttackDataSO Data { get; set; }
        public void Activate();
        public void Deactivate();
        
        void PerformAttack(Vector2 direction);
        
    }
}
