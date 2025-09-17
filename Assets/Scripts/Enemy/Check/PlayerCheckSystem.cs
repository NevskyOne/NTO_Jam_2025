using System;
using UnityEngine;

public class PlayerCheckSystem : MonoBehaviour
{
    [Header("Радиусы обнаружения")]
    [SerializeField] private float _detectionRadius = 8f;
    [SerializeField] private float _rangedRadius = 5f;
    [SerializeField] private float _meleeRadius = 1.5f;
    
    [Header("Настройки")]
    [SerializeField] private LayerMask _playerMask = 1;
    [SerializeField] private float _checkInterval = 0.2f;
    
    public Transform CurrentTarget { get; private set; }
    public bool IsDetected { get; private set; }
    public bool IsInRanged { get; private set; }
    public bool IsInMelee { get; private set; }
    
    public event Action<Transform> PlayerDetected;
    public event Action PlayerLost;
    public event Action<Transform> PlayerEnteredRanged;
    public event Action PlayerLeftRanged;
    public event Action<Transform> PlayerEnteredMelee;
    public event Action PlayerLeftMelee;
    
    private float _lastCheckTime;
    
    private void Update()
    {
        if (Time.time - _lastCheckTime < _checkInterval) return;
        _lastCheckTime = Time.time;
        
        CheckPlayerDetection();
    }
    
    private void CheckPlayerDetection()
    {
        if (CurrentTarget == null)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerMask);
            if (hit != null)
            {
                CurrentTarget = hit.transform;
                IsDetected = true;
                PlayerDetected?.Invoke(CurrentTarget);
            }
            return;
        }
        
        float distance = Vector2.Distance(transform.position, CurrentTarget.position);
        
        if (distance > _detectionRadius)
        {
            if (IsInMelee)
            {
                IsInMelee = false;
                PlayerLeftMelee?.Invoke();
            }
            
            if (IsInRanged)
            {
                IsInRanged = false;
                PlayerLeftRanged?.Invoke();
            }
            
            if (IsDetected)
            {
                IsDetected = false;
                CurrentTarget = null;
                PlayerLost?.Invoke();
            }
            
            return;
        }
        
        bool inMelee = distance <= _meleeRadius;
        if (inMelee != IsInMelee)
        {
            IsInMelee = inMelee;
            if (inMelee)
                PlayerEnteredMelee?.Invoke(CurrentTarget);
            else
                PlayerLeftMelee?.Invoke();
        }
        
        bool inRanged = distance <= _rangedRadius;
        if (inRanged != IsInRanged)
        {
            IsInRanged = inRanged;
            if (inRanged)
                PlayerEnteredRanged?.Invoke(CurrentTarget);
            else
                PlayerLeftRanged?.Invoke();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _rangedRadius);
        
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _meleeRadius);
    }
}
