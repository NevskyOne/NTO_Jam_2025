using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Interfaces
{

    public abstract class EffectBase : ScriptableObject
    {
        [Header("Configuration")]
        [SerializeField] private Core.Data.ScriptableObjects.EffectDataSO _config;
        public Core.Data.ScriptableObjects.EffectDataSO Config => _config;

        public virtual bool AutoExpire => true;

        public void ApplyEffect(GameObject target)
        {
            if (target == null) return;
            var runner = target.GetComponent<EffectRunner>();
            if (runner == null) runner = target.AddComponent<EffectRunner>();
            runner.Apply(this);
        }

        public void RemoveEffect(GameObject target)
        {
            if (target == null) return;
            var runner = target.GetComponent<EffectRunner>();
            if (runner != null) runner.Remove(this);
        }

      
        public virtual void OnApply(GameObject target) {}
        public virtual void OnRemove(GameObject target) {}
    }


    public class EffectRunner : MonoBehaviour
    {
        private readonly Dictionary<EffectBase, Coroutine> _timers = new();
        private readonly Dictionary<EffectBase, GameObject> _spawnedVfx = new();
        private readonly HashSet<EffectBase> _active = new();

        public void Apply(EffectBase effect)
        {
            if (effect == null) return;
            var cfg = effect.Config;

            if (cfg != null && cfg.VisualEffectPrefab != null)
            {
             
                if (_spawnedVfx.TryGetValue(effect, out var prev) && prev != null)
                {
                    Destroy(prev);
                }
                var vfx = Instantiate(cfg.VisualEffectPrefab, transform);
            
                var vfxRenderer = vfx.GetComponentInChildren<SpriteRenderer>();
                if (vfxRenderer != null)
                    vfxRenderer.color = cfg.EffectColor;
                _spawnedVfx[effect] = vfx;
            }

    

            effect.OnApply(gameObject);
            _active.Add(effect);

            if (cfg != null && effect.AutoExpire && cfg.Duration > 0f)
            {
         
                if (_timers.TryGetValue(effect, out var old))
                {
                    StopCoroutine(old);
                    _timers.Remove(effect);
                }
                var co = StartCoroutine(Expire(effect, cfg.Duration));
                _timers[effect] = co;
            }
        }

        public void Remove(EffectBase effect)
        {
            if (effect == null) return;
            if (_timers.TryGetValue(effect, out var co))
            {
                StopCoroutine(co);
                _timers.Remove(effect);
            }

            effect.OnRemove(gameObject);
            _active.Remove(effect);

            if (_spawnedVfx.TryGetValue(effect, out var vfx))
            {
                if (vfx != null) Destroy(vfx);
                _spawnedVfx.Remove(effect);
            }
        }

        private IEnumerator Expire(EffectBase effect, float duration)
        {
            yield return new WaitForSeconds(duration);
            Remove(effect);
        }

       
        public bool HasEffectByName(string effectName)
        {
            if (string.IsNullOrEmpty(effectName)) return false;
            foreach (var e in _active)
            {
                if (e != null && e.Config != null && e.Config.EffectName == effectName)
                    return true;
            }
            return false;
        }
        
        public bool HasEffect<T>() where T : EffectBase
        {
            foreach (var e in _active)
            {
                if (e is T) return true;
            }
            return false;
        }
        
        public T GetEffect<T>() where T : EffectBase
        {
            foreach (var e in _active)
            {
                if (e is T effect) return effect;
            }
            return null;
        }
    }

    // Простые эффекты прямо здесь
    [CreateAssetMenu(fileName = "SpeedEffect", menuName = "Game Data/Effects/Speed Effect")]
    public class SpeedEffect : EffectBase
    {
        [SerializeField] private float _speedMultiplier = 2f;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetSpeedMultiplier(_speedMultiplier);
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetSpeedMultiplier(1f);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "StunEffect", menuName = "Game Data/Effects/Stun Effect")]
    public class StunEffect : EffectBase
    {
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetStunned(true);
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetStunned(false);
            }
        }
    }
    
    [CreateAssetMenu(fileName = "PoisonEffect", menuName = "Game Data/Effects/Poison Effect")]
    public class PoisonEffect : EffectBase
    {
        [SerializeField] private int _damagePerTick = 1;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null)
                player.StartCoroutine(PoisonTick(target));
        }
        
        private IEnumerator PoisonTick(GameObject target)
        {
            var runner = target.GetComponent<EffectRunner>();
            while (runner != null && runner.HasEffect<PoisonEffect>())
            {
                yield return new WaitForSeconds(1f);
                var hittable = target.GetComponent<IHittable>();
                hittable?.TakeDamage(_damagePerTick);
            }
        }
    }
}
