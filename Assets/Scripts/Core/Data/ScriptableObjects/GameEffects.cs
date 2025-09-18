using UnityEngine;
using Core.Interfaces;
using System.Collections;

namespace Core.Data.ScriptableObjects
{
    // Эффект скорости (Чай)
    [CreateAssetMenu(fileName = "SpeedEffect", menuName = "Game Data/Effects/Speed Effect")]
    public class SpeedEffect : EffectBase
    {
        [Header("Speed Settings")]
        [SerializeField] private float _speedMultiplier = 2f;
        [SerializeField] private float _defaultDuration = 5f;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetSpeedMultiplier(_speedMultiplier);
                
                if (Config == null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
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
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект замедления (Лед)
    [CreateAssetMenu(fileName = "SlowEffect", menuName = "Game Data/Effects/Slow Effect")]
    public class SlowEffect : EffectBase
    {
        [Header("Slow Settings")]
        [SerializeField] private float _slowMultiplier = 0.5f;
        [SerializeField] private float _defaultDuration = 3f;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetSpeedMultiplier(_slowMultiplier);
                
                if (Config == null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
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
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект яда (от Шершня)
    [CreateAssetMenu(fileName = "PoisonEffect", menuName = "Game Data/Effects/Poison Effect")]
    public class PoisonEffect : EffectBase
    {
        [Header("Poison Settings")]
        [SerializeField] private int _damagePerTick = 1;
        [SerializeField] private float _tickInterval = 1f;
        [SerializeField] private float _defaultDuration = 5f;
        
        private Coroutine _poisonCoroutine;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null)
            {
                _poisonCoroutine = player.StartCoroutine(PoisonTick(target));
                
                if (Config == null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (_poisonCoroutine != null && player != null)
            {
                player.StopCoroutine(_poisonCoroutine);
            }
        }
        
        private IEnumerator PoisonTick(GameObject target)
        {
            var runner = target.GetComponent<EffectRunner>();
            while (runner != null && runner.HasEffect<PoisonEffect>())
            {
                yield return new WaitForSeconds(_tickInterval);
                var hittable = target.GetComponent<IHittable>();
                hittable?.TakeDamage(_damagePerTick);
            }
        }
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект стана (от Механического паука)
    [CreateAssetMenu(fileName = "StunEffect", menuName = "Game Data/Effects/Stun Effect")]
    public class StunEffect : EffectBase
    {
        [SerializeField] private float _defaultDuration = 2f;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null && player.Movement != null)
            {
                player.Movement.SetStunned(true);
                
                if (Config == null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
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
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект щита (Драконий фрукт)
    [CreateAssetMenu(fileName = "ShieldEffect", menuName = "Game Data/Effects/Shield Effect")]
    public class ShieldEffect : EffectBase
    {
        [Header("Shield Settings")]
        [SerializeField] private int _shieldAmount = 1;
        [SerializeField] private float _defaultDuration = 10f;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            Debug.Log($"🛡️ Щит применен на {target.name}");
            
            if (Config == null)
            {
                var player = target.GetComponent<Player>();
                if (player != null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            Debug.Log($"🛡️ Щит снят с {target.name}");
        }
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект ловушек (Пельмени)
    [CreateAssetMenu(fileName = "TrapEffect", menuName = "Game Data/Effects/Trap Effect")]
    public class TrapEffect : EffectBase
    {
        [Header("Trap Settings")]
        [SerializeField] private float _slowMultiplier = 0.3f;
        [SerializeField] private float _defaultDuration = 4f;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            // Замедляем врага
            var enemy = target.GetComponent<BasicEnemyMovementLogic>();
            if (enemy != null)
            {
                // Здесь нужно добавить метод SetSpeedMultiplier в BasicEnemyMovementLogic
                Debug.Log($"🕸️ Враг {target.name} попал в ловушку!");
            }
            
            if (Config == null)
            {
                var player = GameObject.FindObjectOfType<Player>();
                if (player != null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            Debug.Log($"🕸️ Враг {target.name} освободился от ловушки!");
        }
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект жирности (Корейская морковка)
    [CreateAssetMenu(fileName = "FatEffect", menuName = "Game Data/Effects/Fat Effect")]
    public class FatEffect : EffectBase
    {
        [Header("Fat Settings")]
        [SerializeField] private float _damageReduction = 0.5f;
        [SerializeField] private float _defaultDuration = 8f;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            Debug.Log($"🍖 Жирность применена на {target.name} - снижение урона на {(1-_damageReduction)*100}%");
            
            if (Config == null)
            {
                var player = target.GetComponent<Player>();
                if (player != null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            Debug.Log($"🍖 Жирность снята с {target.name}");
        }
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект тентаклей (Рататуй)
    [CreateAssetMenu(fileName = "TentacleEffect", menuName = "Game Data/Effects/Tentacle Effect")]
    public class TentacleEffect : EffectBase
    {
        [Header("Tentacle Settings")]
        [SerializeField] private int _tentacleCount = 3;
        [SerializeField] private float _tentacleRange = 2f;
        [SerializeField] private int _tentacleDamage = 1;
        [SerializeField] private float _attackInterval = 2f;
        [SerializeField] private float _defaultDuration = 10f;
        
        private Coroutine _tentacleCoroutine;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (player != null)
            {
                _tentacleCoroutine = player.StartCoroutine(TentacleAttack(target));
                
                if (Config == null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            var player = target.GetComponent<Player>();
            if (_tentacleCoroutine != null && player != null)
            {
                player.StopCoroutine(_tentacleCoroutine);
            }
        }
        
        private IEnumerator TentacleAttack(GameObject owner)
        {
            var runner = owner.GetComponent<EffectRunner>();
            while (runner != null && runner.HasEffect<TentacleEffect>())
            {
                yield return new WaitForSeconds(_attackInterval);
                
                // Находим врагов в радиусе
                Collider2D[] enemies = Physics2D.OverlapCircleAll(owner.transform.position, _tentacleRange);
                int attacked = 0;
                
                foreach (var enemy in enemies)
                {
                    if (attacked >= _tentacleCount) break;
                    
                    var hittable = enemy.GetComponent<IHittable>();
                    if (hittable != null && enemy.gameObject != owner)
                    {
                        hittable.TakeDamage(_tentacleDamage);
                        attacked++;
                        Debug.Log($"🐙 Тентакль атаковал {enemy.name}!");
                    }
                }
            }
        }
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект ожирения (Бургер)
    [CreateAssetMenu(fileName = "FattenEffect", menuName = "Game Data/Effects/Fatten Effect")]
    public class FattenEffect : EffectBase
    {
        [Header("Fatten Settings")]
        [SerializeField] private float _sizeMultiplier = 1.5f;
        [SerializeField] private float _defaultDuration = 6f;
        
        private Vector3 _originalScale;
        
        public override bool AutoExpire => true;
        
        public override void OnApply(GameObject target)
        {
            _originalScale = target.transform.localScale;
            target.transform.localScale = _originalScale * _sizeMultiplier;
            Debug.Log($"🍔 Враг {target.name} растолстел!");
            
            if (Config == null)
            {
                var player = GameObject.FindObjectOfType<Player>();
                if (player != null)
                {
                    player.StartCoroutine(ForceExpire(target, _defaultDuration));
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            if (target != null)
            {
                target.transform.localScale = _originalScale;
                Debug.Log($"🍔 Враг {target.name} вернулся к нормальному размеру!");
            }
        }
        
        private IEnumerator ForceExpire(GameObject target, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveEffect(target);
        }
    }
    
    // Эффект взрыва (Взрывная карамель)
    [CreateAssetMenu(fileName = "BombEffect", menuName = "Game Data/Effects/Bomb Effect")]
    public class BombEffect : EffectBase
    {
        [Header("Bomb Settings")]
        [SerializeField] private int _explosionDamage = 3;
        [SerializeField] private float _explosionRadius = 2f;
        [SerializeField] private float _fuseTime = 2f;
        
        public override bool AutoExpire => false; // Взрывается сам
        
        public override void OnApply(GameObject target)
        {
            var player = GameObject.FindObjectOfType<Player>();
            if (player != null)
            {
                player.StartCoroutine(BombCountdown(target));
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            // Эффект удаляется автоматически после взрыва
        }
        
        private IEnumerator BombCountdown(GameObject target)
        {
            Debug.Log($"💣 Бомба установлена на {target.name}! Взрыв через {_fuseTime} сек!");
            
            yield return new WaitForSeconds(_fuseTime);
            
            if (target != null)
            {
                // Взрыв!
                Collider2D[] victims = Physics2D.OverlapCircleAll(target.transform.position, _explosionRadius);
                
                foreach (var victim in victims)
                {
                    var hittable = victim.GetComponent<IHittable>();
                    if (hittable != null)
                    {
                        hittable.TakeDamage(_explosionDamage);
                        Debug.Log($"💥 Взрыв повредил {victim.name}!");
                    }
                }
                
                Debug.Log($"💥 ВЗРЫВ! Урон {_explosionDamage} в радиусе {_explosionRadius}");
                RemoveEffect(target);
            }
        }
    }
}
