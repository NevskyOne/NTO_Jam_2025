using UnityEngine;
using Core.Interfaces;

namespace Core.Data.ScriptableObjects
{
    // Эффект скорости (Чай)
    [CreateAssetMenu(fileName = "SpeedEffect", menuName = "Game Data/Effects/Speed Effect")]
    public class SpeedEffect : EffectBase
    {
        [Header("Speed Settings")]
        [SerializeField] private float _speedMultiplier = 2f;
        
        private PlayerMovementLogic _playerMovement;
        private float _originalSpeed;
        
        public override void OnApply(GameObject target)
        {
            _playerMovement = target.GetComponent<PlayerMovementLogic>();
            if (_playerMovement != null)
            {
                _originalSpeed = _playerMovement.CurrentMoveSpeed;
                _playerMovement.SetSpeedMultiplier(_speedMultiplier);
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            if (_playerMovement != null)
            {
                _playerMovement.SetSpeedMultiplier(1f);
            }
        }
    }
    
    // Эффект замедления (Айс латте)
    [CreateAssetMenu(fileName = "SlowEffect", menuName = "Game Data/Effects/Slow Effect")]
    public class SlowEffect : EffectBase
    {
        [Header("Slow Settings")]
        [SerializeField] private float _slowMultiplier = 0.5f;
        
        private PlayerMovementLogic _playerMovement;
        
        public override void OnApply(GameObject target)
        {
            _playerMovement = target.GetComponent<PlayerMovementLogic>();
            if (_playerMovement != null)
            {
                _playerMovement.SetSpeedMultiplier(_slowMultiplier);
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            if (_playerMovement != null)
            {
                _playerMovement.SetSpeedMultiplier(1f);
            }
        }
    }
    
    // Эффект щита (Драконий фрукт)
    [CreateAssetMenu(fileName = "ShieldEffect", menuName = "Game Data/Effects/Shield Effect")]
    public class ShieldEffect : EffectBase
    {
        [Header("Shield Settings")]
        [SerializeField] private int _shieldAmount = 1;
        
        private Player _player;
        private int _currentShield;
        
        public override void OnApply(GameObject target)
        {
            _player = target.GetComponent<Player>();
            if (_player != null)
            {
                _currentShield = _shieldAmount;
                // Подписываемся на урон чтобы блокировать его
                _player.GetComponent<IHittable>();
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            _currentShield = 0;
        }
        
        public bool TryBlockDamage()
        {
            if (_currentShield > 0)
            {
                _currentShield--;
                if (_currentShield <= 0)
                {
                    // Щит исчерпан, удаляем эффект
                    if (_player != null)
                        RemoveEffect(_player.gameObject);
                }
                return true;
            }
            return false;
        }
    }
    
    // Эффект липких ловушек (Пельмени)
    [CreateAssetMenu(fileName = "TrapEffect", menuName = "Game Data/Effects/Trap Effect")]
    public class TrapEffect : EffectBase
    {
        [Header("Trap Settings")]
        [SerializeField] private float _slowAmount = 0.3f;
        
        private IEnemyMovable _enemyMovement;
        
        public override void OnApply(GameObject target)
        {
            _enemyMovement = target.GetComponent<IEnemyMovable>();
            if (_enemyMovement != null)
            {
                // Замедляем врага
                var basicMovement = target.GetComponent<BasicEnemyMovementLogic>();
                if (basicMovement != null)
                {
                    // Здесь нужно добавить метод для изменения скорости в BasicEnemyMovementLogic
                }
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            // Восстанавливаем скорость
        }
    }
    
    // Эффект жирности (Корейская морковка)
    [CreateAssetMenu(fileName = "FatEffect", menuName = "Game Data/Effects/Fat Effect")]
    public class FatEffect : EffectBase
    {
        [Header("Fat Settings")]
        [SerializeField] private float _damageReduction = 0.5f;
        
        private Player _player;
        
        public override void OnApply(GameObject target)
        {
            _player = target.GetComponent<Player>();
            // Эффект снижения урона будет обрабатываться в Player.TakeDamage
        }
        
        public override void OnRemove(GameObject target)
        {
            // Восстанавливаем нормальный урон
        }
        
        public float GetDamageMultiplier()
        {
            return _damageReduction;
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
        
        private Player _player;
        private Coroutine _tentacleCoroutine;
        
        public override void OnApply(GameObject target)
        {
            _player = target.GetComponent<Player>();
            if (_player != null)
            {
                _tentacleCoroutine = _player.StartCoroutine(TentacleAttack());
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            if (_tentacleCoroutine != null && _player != null)
            {
                _player.StopCoroutine(_tentacleCoroutine);
            }
        }
        
        private System.Collections.IEnumerator TentacleAttack()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f); // Атака каждые 2 секунды
                
                // Находим врагов в радиусе
                Collider2D[] enemies = Physics2D.OverlapCircleAll(_player.transform.position, _tentacleRange);
                int attacked = 0;
                
                foreach (var enemy in enemies)
                {
                    if (attacked >= _tentacleCount) break;
                    
                    var hittable = enemy.GetComponent<IHittable>();
                    if (hittable != null && enemy.gameObject != _player.gameObject)
                    {
                        hittable.TakeDamage(_tentacleDamage);
                        attacked++;
                    }
                }
            }
        }
    }
    
    // Эффект яда (от Шершня)
    [CreateAssetMenu(fileName = "PoisonEffect", menuName = "Game Data/Effects/Poison Effect")]
    public class PoisonEffect : EffectBase
    {
        [Header("Poison Settings")]
        [SerializeField] private int _damagePerTick = 1;
        [SerializeField] private float _tickInterval = 1f;
        
        private Player _player;
        private Coroutine _poisonCoroutine;
        
        public override void OnApply(GameObject target)
        {
            _player = target.GetComponent<Player>();
            if (_player != null)
            {
                _poisonCoroutine = _player.StartCoroutine(PoisonTick());
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            if (_poisonCoroutine != null && _player != null)
            {
                _player.StopCoroutine(_poisonCoroutine);
            }
        }
        
        private System.Collections.IEnumerator PoisonTick()
        {
            while (true)
            {
                yield return new WaitForSeconds(_tickInterval);
                if (_player != null)
                {
                    _player.TakeDamage(_damagePerTick);
                }
            }
        }
    }
    
    // Эффект стана (от Механического паука)
    [CreateAssetMenu(fileName = "StunEffect", menuName = "Game Data/Effects/Stun Effect")]
    public class StunEffect : EffectBase
    {
        private Player _player;
        private PlayerMovementLogic _movement;
        
        public override void OnApply(GameObject target)
        {
            _player = target.GetComponent<Player>();
            _movement = target.GetComponent<PlayerMovementLogic>();
            
            if (_movement != null)
            {
                _movement.SetStunned(true);
            }
        }
        
        public override void OnRemove(GameObject target)
        {
            if (_movement != null)
            {
                _movement.SetStunned(false);
            }
        }
    }
}
