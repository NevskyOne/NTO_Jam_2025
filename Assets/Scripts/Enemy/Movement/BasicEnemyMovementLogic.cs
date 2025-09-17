using Core.Interfaces;
using UnityEngine;

public class BasicEnemyMovementLogic : MonoBehaviour, IEnemyMovable
{
    [SerializeField] private BasicEnemyMoveDataSO _moveData;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    private Transform _target;
    private bool _movementEnabled = true;
    
    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody2D>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void FixedUpdate()
    {
        if (!_movementEnabled || _target == null || _moveData == null || _rigidbody == null) return;
        
        Vector2 direction = (_target.position - transform.position).normalized;
        Vector2 targetVelocity = direction * _moveData.MoveSpeed;
        
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
    
    public void SetTarget(Transform target)
    {
        _target = target;
    }
    
    public void ClearTarget()
    {
        _target = null;
        if (_rigidbody != null && _moveData != null)
        {
            _rigidbody.linearVelocity = Vector2.MoveTowards(
                _rigidbody.linearVelocity, 
                Vector2.zero, 
                _moveData.Deceleration * Time.fixedDeltaTime
            );
        }
    }
    
    public void EnableMovement(bool enabled)
    {
        _movementEnabled = enabled;
        if (!enabled && _rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}
