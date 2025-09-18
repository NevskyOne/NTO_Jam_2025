using Core.Interfaces;
using UnityEngine;

public class BasicEnemyMovementLogic : MonoBehaviour, IEnemyMovable
{
    [SerializeField] private BasicEnemyMoveDataSO _moveData;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Header("Movement Type")]
    [SerializeField] private bool _canFly = false;
    [SerializeField] private float _flyHeight = 2f;
    [SerializeField] private float _minDistanceToTarget = 0.5f;
    
    private Transform _target;
    private bool _movementEnabled = true;
    private Vector3 _originalPosition;
    
    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody2D>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
        _originalPosition = transform.position;
        
        // Настройка физики для летающих врагов
        if (_canFly && _rigidbody != null)
        {
            _rigidbody.gravityScale = 0f;
        }
    }
    
    private void FixedUpdate()
    {
        if (!_movementEnabled || _moveData == null || _rigidbody == null) return;
        
        if (_target != null)
        {
            MoveToTarget(_target.position);
        }
        else
        {
            // Если нет цели, возвращаемся к исходной позиции
            ReturnToOriginalPosition();
        }
    }
    
    private void MoveToTarget(Vector2 targetPos)
    {
        Vector2 currentPos = transform.position;
        
        // Для летающих врагов добавляем высоту
        if (_canFly)
        {
            targetPos.y += _flyHeight;
        }
        
        float distanceToTarget = Vector2.Distance(currentPos, targetPos);
        
        // Если враг достаточно близко, продолжаем двигаться к цели (но медленнее)
        Vector2 direction = (targetPos - currentPos).normalized;
        float speedMultiplier = 1f;
        
        // Если очень близко к цели, замедляемся но не останавливаемся
        if (distanceToTarget < _minDistanceToTarget)
        {
            speedMultiplier = 0.3f; // Замедляемся до 30% от обычной скорости
        }
        
        Vector2 targetVelocity = direction * _moveData.MoveSpeed * speedMultiplier;
        
        _rigidbody.linearVelocity = Vector2.MoveTowards(
            _rigidbody.linearVelocity, 
            targetVelocity, 
            _moveData.Acceleration * Time.fixedDeltaTime
        );
        
        // Поворот спрайта
        if (_moveData.FlipSprite && _spriteRenderer != null && direction.x != 0)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    private void ReturnToOriginalPosition()
    {
        Vector2 currentPos = transform.position;
        Vector2 originalPos = _originalPosition;
        
        // Для летающих врагов добавляем высоту к исходной позиции
        if (_canFly)
        {
            originalPos.y += _flyHeight;
        }
        
        float distanceToOriginal = Vector2.Distance(currentPos, originalPos);
        
        // Если далеко от исходной позиции, возвращаемся
        if (distanceToOriginal > 0.5f)
        {
            Vector2 direction = (originalPos - currentPos).normalized;
            Vector2 targetVelocity = direction * _moveData.MoveSpeed * 0.5f; // Медленнее возвращаемся
            
            _rigidbody.linearVelocity = Vector2.MoveTowards(
                _rigidbody.linearVelocity, 
                targetVelocity, 
                _moveData.Acceleration * Time.fixedDeltaTime
            );
            
            if (_moveData.FlipSprite && _spriteRenderer != null && direction.x != 0)
            {
                _spriteRenderer.flipX = direction.x < 0;
            }
        }
        else
        {
            // Если близко к исходной позиции, останавливаемся
            _rigidbody.linearVelocity = Vector2.MoveTowards(
                _rigidbody.linearVelocity, 
                Vector2.zero, 
                _moveData.Deceleration * Time.fixedDeltaTime
            );
        }
    }
    
    public void SetTarget(Transform target)
    {
        _target = target;
    }
    
    public void ClearTarget()
    {
        _target = null;
    }
    
    public void EnableMovement(bool enabled)
    {
        _movementEnabled = enabled;
        if (!enabled && _rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
    
    // Публичные методы для настройки
    public void SetCanFly(bool canFly)
    {
        _canFly = canFly;
        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = canFly ? 0f : 1f;
        }
    }
    
    public void SetFlyHeight(float height)
    {
        _flyHeight = height;
    }
    
    public void SetMinDistanceToTarget(float distance)
    {
        _minDistanceToTarget = distance;
    }
    
    public bool IsFlying => _canFly;
    public float FlyHeight => _flyHeight;
    
    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        // Показываем исходную позицию
        Gizmos.color = Color.blue;
        Vector3 originalPos = Application.isPlaying ? _originalPosition : transform.position;
        Gizmos.DrawWireSphere(originalPos, 0.5f);
        
        // Показываем высоту полета для летающих врагов
        if (_canFly)
        {
            Gizmos.color = Color.cyan;
            Vector3 flyPos = originalPos + Vector3.up * _flyHeight;
            Gizmos.DrawWireSphere(flyPos, 0.3f);
            Gizmos.DrawLine(originalPos, flyPos);
        }
        
        // Показываем минимальную дистанцию до цели
        if (_target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _minDistanceToTarget);
        }
    }
}
