using UnityEngine;

namespace Core.Interfaces
{
    public class EffectBase : ScriptableObject
    {
        
        public virtual void ApplyEffect(GameObject target){}
        public virtual void RemoveEffect(GameObject target){}
    }
}
