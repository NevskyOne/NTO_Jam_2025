using System.Collections;
using UnityEngine;
using Core.Interfaces;

// Дополнительное поведение для Флауи, работает ВМЕСТЕ с EnemyAI
public class FloweyBehavior : MonoBehaviour
{
    [Header("Flowey Shooting")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform[] _shootPoints;
    [SerializeField] private float _bulletSpeed = 6f;
    [SerializeField] private float _shootCooldown = 1f;
    [SerializeField] private int _bulletsPerShot = 3;
    [SerializeField] private float _bulletSpread = 30f;
    
    [Header("Teleportation")]
    [SerializeField] private float _teleportCooldown = 5f;
    [SerializeField] private float _teleportRange = 8f;
    [SerializeField] private LayerMask _groundLayer = 1;
    
    [Header("Room Bullets")]
    [SerializeField] private float _roomBulletCooldown = 3f;
    [SerializeField] private int _roomBulletsCount = 5;
    [SerializeField] private float _roomBulletLifetime = 8f;
    
    private EnemyAI _enemyAI;
    private PlayerCheckSystem _playerCheck;
    private BasicEnemyMovementLogic _movement;
    private Coroutine _shootCoroutine;
    private Coroutine _teleportCoroutine;
    private Coroutine _roomBulletCoroutine;
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _playerCheck = GetComponent<PlayerCheckSystem>();
        _movement = GetComponent<BasicEnemyMovementLogic>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Флауи неподвижен - отключаем движение
        if (_movement != null)
        {
            _movement.EnableMovement(false);
        }
        
        // Создаем точки стрельбы если их нет
        if (_shootPoints == null || _shootPoints.Length == 0)
        {
            CreateShootPoints();
        }
    }
    
    private void CreateShootPoints()
    {
        _shootPoints = new Transform[4];
        string[] directions = { "Up", "Down", "Left", "Right" };
        Vector3[] positions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject shootPoint = new GameObject($"ShootPoint_{directions[i]}");
            shootPoint.transform.SetParent(transform);
            shootPoint.transform.localPosition = positions[i] * 0.5f;
            _shootPoints[i] = shootPoint.transform;
        }
    }
    
    private void OnEnable()
    {
        // Подписываемся на события ПОСЛЕ EnemyAI
        if (_playerCheck != null)
        {
            _playerCheck.PlayerDetected += OnPlayerDetected;
            _playerCheck.PlayerLost += OnPlayerLost;
        }
    }
    
    private void OnDisable()
    {
        if (_playerCheck != null)
        {
            _playerCheck.PlayerDetected -= OnPlayerDetected;
            _playerCheck.PlayerLost -= OnPlayerLost;
        }
        
        StopAllAttacks();
    }
    
    private void OnPlayerDetected(Transform player)
    {
        StartAllAttacks();
    }
    
    private void OnPlayerLost()
    {
        StopAllAttacks();
    }
    
    private void StartAllAttacks()
    {
        if (_shootCoroutine == null)
            _shootCoroutine = StartCoroutine(ShootingRoutine());
            
        if (_teleportCoroutine == null)
            _teleportCoroutine = StartCoroutine(TeleportRoutine());
            
        if (_roomBulletCoroutine == null)
            _roomBulletCoroutine = StartCoroutine(RoomBulletRoutine());
    }
    
    private void StopAllAttacks()
    {
        if (_shootCoroutine != null)
        {
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }
        
        if (_teleportCoroutine != null)
        {
            StopCoroutine(_teleportCoroutine);
            _teleportCoroutine = null;
        }
        
        if (_roomBulletCoroutine != null)
        {
            StopCoroutine(_roomBulletCoroutine);
            _roomBulletCoroutine = null;
        }
    }
    
    private IEnumerator ShootingRoutine()
    {
        while (_playerCheck != null && _playerCheck.CurrentTarget != null)
        {
            yield return new WaitForSeconds(_shootCooldown);
            ShootAtPlayer(_playerCheck.CurrentTarget);
        }
        
        _shootCoroutine = null;
    }
    
    private IEnumerator TeleportRoutine()
    {
        while (_playerCheck != null && _playerCheck.CurrentTarget != null)
        {
            yield return new WaitForSeconds(_teleportCooldown);
            PerformTeleport();
        }
        
        _teleportCoroutine = null;
    }
    
    private IEnumerator RoomBulletRoutine()
    {
        while (_playerCheck != null && _playerCheck.CurrentTarget != null)
        {
            yield return new WaitForSeconds(_roomBulletCooldown);
            SpawnRoomBullets();
        }
        
        _roomBulletCoroutine = null;
    }
    
    private void ShootAtPlayer(Transform target)
    {
        if (_bulletPrefab == null || _shootPoints.Length == 0) return;
        
        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        
        // Выбираем ближайшую точку стрельбы
        Transform bestShootPoint = _shootPoints[0];
        float bestDot = Vector2.Dot(directionToPlayer, (_shootPoints[0].position - transform.position).normalized);
        
        foreach (var shootPoint in _shootPoints)
        {
            Vector2 shootDir = (shootPoint.position - transform.position).normalized;
            float dot = Vector2.Dot(directionToPlayer, shootDir);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestShootPoint = shootPoint;
            }
        }
        
        // Стреляем несколькими пулями с разбросом
        for (int i = 0; i < _bulletsPerShot; i++)
        {
            float spreadAngle = Random.Range(-_bulletSpread, _bulletSpread);
            Vector2 shootDirection = Quaternion.Euler(0, 0, spreadAngle) * directionToPlayer;
            
            CreateBullet(bestShootPoint.position, shootDirection, _bulletSpeed);
        }
        
        Debug.Log("🌻 Флауи стреляет в игрока!");
    }
    
    private void SpawnRoomBullets()
    {
        for (int i = 0; i < _roomBulletsCount; i++)
        {
            Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * _teleportRange;
            
            if (IsValidPosition(randomPos))
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                CreateBullet(randomPos, randomDirection, _bulletSpeed * 0.7f, _roomBulletLifetime);
            }
        }
        
        Debug.Log("🌻💫 Флауи создает пули по всей комнате!");
    }
    
    private void CreateBullet(Vector2 position, Vector2 direction, float speed, float lifetime = 5f)
    {
        GameObject bullet = Instantiate(_bulletPrefab, position, Quaternion.identity);
        
        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        rb.linearVelocity = direction * speed;
        
        var bulletScript = bullet.GetComponent<FloweyBullet>();
        if (bulletScript == null)
        {
            bulletScript = bullet.AddComponent<FloweyBullet>();
        }
        bulletScript.Initialize(1, lifetime);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private void PerformTeleport()
    {
        Vector2 newPosition = FindValidTeleportPosition();
        
        if (newPosition != Vector2.zero)
        {
            StartCoroutine(TeleportEffect(newPosition));
        }
    }
    
    private IEnumerator TeleportEffect(Vector2 newPosition)
    {
        if (_spriteRenderer != null)
        {
            Color originalColor = _spriteRenderer.color;
            
            // Мигание перед телепортацией
            for (int i = 0; i < 3; i++)
            {
                _spriteRenderer.color = Color.clear;
                yield return new WaitForSeconds(0.1f);
                _spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            
            transform.position = newPosition;
            
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.2f);
            _spriteRenderer.color = originalColor;
        }
        else
        {
            transform.position = newPosition;
        }
        
        Debug.Log("🌻✨ Флауи телепортировался!");
    }
    
    private Vector2 FindValidTeleportPosition()
    {
        for (int attempts = 0; attempts < 10; attempts++)
        {
            Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * _teleportRange;
            
            if (IsValidPosition(randomPos))
            {
                return randomPos;
            }
        }
        
        return Vector2.zero;
    }
    
    private bool IsValidPosition(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, 0.5f, _groundLayer);
        return hit == null;
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _teleportRange);
        
        if (_shootPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var shootPoint in _shootPoints)
            {
                if (shootPoint != null)
                {
                    Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
                    Gizmos.DrawLine(transform.position, shootPoint.position);
                }
            }
        }
    }
}

public class FloweyBullet : MonoBehaviour
{
    private int _damage;
    private bool _hasHit = false;
    
    public void Initialize(int damage, float lifetime)
    {
        _damage = damage;
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
            hittable?.TakeDamage(_damage);
            
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
