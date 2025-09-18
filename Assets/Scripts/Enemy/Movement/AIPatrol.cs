using UnityEngine;
using Pathfinding;
using Core.Interfaces;
using System.Collections;

[RequireComponent(typeof(IAstarAI))]
public class AIPatrol : MonoBehaviour, IEnemyMovable
{
    [Header("Patrol Settings")]
    [SerializeField] private float _patrolRadius = 8f;
    [SerializeField] private float _waitTime = 2f;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private bool _canFly = false;
    
    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private bool _flipSprite = true;
    
    private IAstarAI _ai;
    private Transform _target;
    private Vector3 _startPosition;
    private bool _isPatrolling = false;
    private bool _movementEnabled = true;
    private Coroutine _patrolCoroutine;
    
    private void Awake()
    {
        _ai = GetComponent<IAstarAI>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
        _startPosition = transform.position;
        
        // Настройка AI
        if (_ai != null)
        {
            _ai.maxSpeed = _moveSpeed;
            _ai.canMove = true;
        }
    }
    
    private void Start()
    {
        if (_movementEnabled)
            StartPatrol();
    }
    
    private void Update()
    {
        if (_ai == null) return;
        
        // Поворот спрайта
        if (_flipSprite && _spriteRenderer != null && _ai.velocity.x != 0)
        {
            _spriteRenderer.flipX = _ai.velocity.x < 0;
        }
        
        // Если есть цель - постоянно обновляем путь к ней
        if (_target != null)
        {
            Vector3 targetPos = _target.position;
            if (_canFly) targetPos.y += 1f;
            
            // Обновляем путь к цели каждый кадр
            if (Vector3.Distance(_ai.destination, targetPos) > 0.5f)
            {
                _ai.destination = targetPos;
            }
        }
    }
    
    public void SetTarget(Transform target)
    {
        _target = target;
        
        // ВАЖНО: Останавливаем патрулирование
        StopPatrol();
        
        // Сразу идем к цели
        if (_target != null && _ai != null)
        {
            Vector3 targetPos = _target.position;
            if (_canFly) targetPos.y += 1f;
            
            _ai.destination = targetPos;
            _ai.isStopped = false;
        }
    }
    
    public void ClearTarget()
    {
        _target = null;
        
        // Возвращаемся к патрулированию
        StartPatrol();
    }
    
    public void EnableMovement(bool enabled)
    {
        _movementEnabled = enabled;
        
        if (_ai != null)
        {
            _ai.canMove = enabled;
            _ai.isStopped = !enabled;
        }
        
        if (enabled && _target == null)
        {
            StartPatrol();
        }
        else if (!enabled)
        {
            StopPatrol();
        }
    }
    
    private void StartPatrol()
    {
        if (!_movementEnabled || _target != null || _isPatrolling) return;
        
        _isPatrolling = true;
        _patrolCoroutine = StartCoroutine(PatrolRoutine());
    }
    
    private void StopPatrol()
    {
        _isPatrolling = false;
        
        if (_patrolCoroutine != null)
        {
            StopCoroutine(_patrolCoroutine);
            _patrolCoroutine = null;
        }
    }
    
    private IEnumerator PatrolRoutine()
    {
        while (_isPatrolling && _target == null && _movementEnabled)
        {
            // Находим новую точку патрулирования
            FindNewPatrolPoint();
            
            // Ждем пока дойдем до точки
            while (_ai.pathPending || _ai.remainingDistance > 0.5f)
            {
                // Если появилась цель - прерываем патруль
                if (_target != null || !_isPatrolling)
                    yield break;
                    
                yield return null;
            }
            
            // Ждем в точке
            _ai.isStopped = true;
            yield return new WaitForSeconds(_waitTime);
            _ai.isStopped = false;
            
            // Проверяем еще раз перед следующей итерацией
            if (_target != null || !_isPatrolling)
                yield break;
        }
        
        _isPatrolling = false;
    }
    
    private void FindNewPatrolPoint()
    {
        Vector2 randomDir = Random.insideUnitCircle * _patrolRadius;
        Vector3 newPoint = _startPosition + new Vector3(randomDir.x, randomDir.y, 0);
        
        if (_canFly) 
            newPoint.y += Random.Range(0.5f, 2f);
        
        _ai.destination = newPoint;
    }
    
    // Визуализация
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? _startPosition : transform.position;
        
        // Радиус патрулирования
        Gizmos.color = _isPatrolling ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(center, _patrolRadius);
        
        // Цель преследования
        if (_target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.position);
            Gizmos.DrawWireSphere(_target.position, 0.5f);
        }
        
        // Текущая точка назначения
        if (_ai != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_ai.destination, 0.3f);
        }
    }
}