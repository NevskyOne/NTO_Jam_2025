using Core.Data.ScriptableObjects;
using UnityEngine;

namespace Core.Interfaces
{

    public interface IAttack
    {
        // Serialization of Data is done in concrete classes (e.g., via [SerializeField] backing field)
        public AttackDataSO Data { get; set; }
        public void Activate();
        public void Deactivate();
        
        void PerformAttack(Vector2 direction);
        
    }
}
