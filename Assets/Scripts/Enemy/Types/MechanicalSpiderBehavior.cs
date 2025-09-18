using System.Collections;
using UnityEngine;
using Core.Interfaces;

// Дополнительное поведение для механического паука, работает ВМЕСТЕ с EnemyAI
public class MechanicalSpiderBehavior : MonoBehaviour
{
    [Header("Spider Tail Attack")]
    [SerializeField] private Transform _tailAttackPoint;
    [SerializeField] private float _tailAttackRange = 2f;
    [SerializeField] private float _tailAttackCooldown = 1.5f;
    [SerializeField] private int _tailDamage = 2;
    [SerializeField] private EffectBase _stunEffect;
    [SerializeField] private float _attackWindupTime = 0.5f;
    
    private EnemyAI _enemyAI;
    private PlayerCheckSystem _playerCheck;
    private BasicEnemyMovementLogic _movement;
    private Coroutine _attackCoroutine;
    private bool _isAttacking = false;
    private Animator _animator;
    
    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _playerCheck = GetComponent<PlayerCheckSystem>();
        _movement = GetComponent<BasicEnemyMovementLogic>();
        _animator = GetComponent<Animator>();
        
        // Настраиваем как наземного врага
        if (_movement != null)
        {
            _movement.SetCanFly(false);
            _movement.SetMinDistanceToTarget(1.2f);
        }
        
        // Создаем точку атаки хвостом если её нет
        if (_tailAttackPoint == null)
        {
            GameObject tailPoint = new GameObject("TailAttackPoint");
            tailPoint.transform.SetParent(transform);
            tailPoint.transform.localPosition = Vector3.back * 0.5f;
            _tailAttackPoint = tailPoint.transform;
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
        
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }
    }
    
    private void OnPlayerInRange(Transform player)
    {
        // Начинаем атаки хвостом когда игрок в дальней зоне
        if (_attackCoroutine == null && !_isAttacking)
        {
            _attackCoroutine = StartCoroutine(TailAttackRoutine());
        }
    }
    
    private void OnPlayerLeftRange()
    {
        // Прекращаем атаки
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
        _isAttacking = false;
    }
    
    private IEnumerator TailAttackRoutine()
    {
        while (_playerCheck != null && _playerCheck.IsInRanged)
        {
            yield return StartCoroutine(PerformTailAttack());
            yield return new WaitForSeconds(_tailAttackCooldown);
        }
        
        _attackCoroutine = null;
    }
    
    private IEnumerator PerformTailAttack()
    {
        _isAttacking = true;
        
        // Останавливаем движение для атаки
        if (_movement != null)
        {
            _movement.EnableMovement(false);
        }
        
        // Анимация подготовки атаки
        if (_animator != null)
        {
            _animator.SetTrigger("TailAttackWindup");
        }
        
        Debug.Log("🕷️ Механический паук готовится к атаке хвостом...");
        yield return new WaitForSeconds(_attackWindupTime);
        
        // Выполняем атаку
        PerformTailStrike();
        
        // Анимация атаки
        if (_animator != null)
        {
            _animator.SetTrigger("TailAttackStrike");
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // Возобновляем движение
        if (_movement != null)
        {
            _movement.EnableMovement(true);
        }
        
        _isAttacking = false;
    }
    
    private void PerformTailStrike()
    {
        if (_tailAttackPoint == null || _playerCheck == null || _playerCheck.CurrentTarget == null) return;
        
        Vector2 attackPosition = _tailAttackPoint.position;
        Vector2 playerPosition = _playerCheck.CurrentTarget.position;
        
        float distanceToPlayer = Vector2.Distance(attackPosition, playerPosition);
        
        if (distanceToPlayer <= _tailAttackRange)
        {
            var hittable = _playerCheck.CurrentTarget.GetComponent<IHittable>();
            if (hittable != null)
            {
                hittable.TakeDamage(_tailDamage);
                
                if (_stunEffect != null)
                {
                    _stunEffect.ApplyEffect(_playerCheck.CurrentTarget.gameObject);
                }
                
                Debug.Log("🕷️⚡ Механический паук ударил хвостом! Игрок оглушен!");
                StartCoroutine(TailStrikeEffect());
            }
        }
        else
        {
            Debug.Log("🕷️ Механический паук промахнулся!");
        }
    }
    
    private IEnumerator TailStrikeEffect()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (_tailAttackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_tailAttackPoint.position, _tailAttackRange);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _tailAttackPoint.position);
        }
        
        if (_playerCheck != null && _playerCheck.CurrentTarget != null)
        {
            Gizmos.color = _isAttacking ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position, _playerCheck.CurrentTarget.position);
        }
    }
}
