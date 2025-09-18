using System.Collections;
using UnityEngine;
using Core.Interfaces;

// Дополнительное поведение для шершня, работает ВМЕСТЕ с EnemyAI
public class HornetEnemyBehavior : MonoBehaviour
{
    [Header("Hornet Shooting")]
    [SerializeField] private GameObject _stingerProjectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _projectileSpeed = 8f;
    [SerializeField] private float _shootCooldown = 2f;
    [SerializeField] private EffectBase _poisonEffect;
    
    private EnemyAI _enemyAI;
    private PlayerCheckSystem _playerCheck;
    private Coroutine _shootCoroutine;
    
    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _playerCheck = GetComponent<PlayerCheckSystem>();
        
        // Настраиваем базовое движение как летающее
        var movement = GetComponent<BasicEnemyMovementLogic>();
        if (movement != null)
        {
            movement.SetCanFly(true);
            movement.SetFlyHeight(2f);
            movement.SetMinDistanceToTarget(3f);
        }
        
        // Создаем точку стрельбы если её нет
        if (_shootPoint == null)
        {
            GameObject shootPoint = new GameObject("ShootPoint");
            shootPoint.transform.SetParent(transform);
            shootPoint.transform.localPosition = Vector3.forward * 0.5f;
            _shootPoint = shootPoint.transform;
        }
    }
    
    private void OnEnable()
    {
        // Подписываемся на события ПОСЛЕ EnemyAI
        if (_playerCheck != null)
        {
            _playerCheck.PlayerEnteredRanged += OnPlayerInRange;
            _playerCheck.PlayerLeftRanged += OnPlayerLeftRange;
        }
    }
    
    private void OnDisable()
    {
        if (_playerCheck != null)
        {
            _playerCheck.PlayerEnteredRanged -= OnPlayerInRange;
            _playerCheck.PlayerLeftRanged -= OnPlayerLeftRange;
        }
        
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
        }
    }
    
    private void OnPlayerInRange(Transform player)
    {
        // Начинаем стрелять когда игрок в дальней зоне
        if (_shootCoroutine == null)
        {
            _shootCoroutine = StartCoroutine(ShootingRoutine());
        }
    }
    
    private void OnPlayerLeftRange()
    {
        // Прекращаем стрелять
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }
    }
    
    private IEnumerator ShootingRoutine()
    {
        while (_playerCheck != null && _playerCheck.IsInRanged)
        {
            yield return new WaitForSeconds(_shootCooldown);
            
            if (_playerCheck.CurrentTarget != null)
            {
                ShootStinger(_playerCheck.CurrentTarget);
            }
        }
        
        _shootCoroutine = null;
    }
    
    private void ShootStinger(Transform target)
    {
        if (_stingerProjectilePrefab == null || _shootPoint == null) return;
        
        Vector2 direction = (target.position - _shootPoint.position).normalized;
        GameObject projectile = Instantiate(_stingerProjectilePrefab, _shootPoint.position, Quaternion.identity);
        
        var rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * _projectileSpeed;
        }
        
        var stinger = projectile.GetComponent<StingerProjectile>();
        if (stinger == null)
        {
            stinger = projectile.AddComponent<StingerProjectile>();
        }
        stinger.Initialize(_poisonEffect, 5f);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        Debug.Log("🐝 Шершень выстрелил жалом!");
    }
}

public class StingerProjectile : MonoBehaviour
{
    private EffectBase _poisonEffect;
    private bool _hasHit = false;
    
    public void Initialize(EffectBase poisonEffect, float lifetime)
    {
        _poisonEffect = poisonEffect;
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            _hasHit = true;
            
            var hittable = other.GetComponent<IHittable>();
            hittable?.TakeDamage(1);
            
            if (_poisonEffect != null)
            {
                _poisonEffect.ApplyEffect(other.gameObject);
            }
            
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
