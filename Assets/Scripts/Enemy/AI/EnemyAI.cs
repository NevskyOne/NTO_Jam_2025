using System.Collections.Generic;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IHittable, IEffectHandler 
{
    [SerializeField] private EnemyDataSO _data;
    [SerializeField] private PlayerCheckSystem _playerCheck;

    private List<EffectBase> _activeEffects = new List<EffectBase>();
    private EnemyState _state = EnemyState.Normal;
    private int _currentHealth;

    private void Awake()
    {
        if (_data != null)
        {
            _currentHealth = _data.MaxHealth;
        }
    }

    public void OnEnable()
    {
        if (_playerCheck != null)
        {
            _playerCheck.PlayerDetected += OnPlayerDetected;
            _playerCheck.PlayerLost += OnPlayerLost;
            _playerCheck.PlayerEnteredRanged += OnPlayerEnteredRanged;
            _playerCheck.PlayerEnteredMelee += OnPlayerEnteredMelee;
            _playerCheck.PlayerLeftRanged += OnPlayerLeftRanged;
            _playerCheck.PlayerLeftMelee += OnPlayerLeftMelee;
        }
    }

    public void OnDisable()
    {
        if (_playerCheck != null)
        {
            _playerCheck.PlayerDetected -= OnPlayerDetected;
            _playerCheck.PlayerLost -= OnPlayerLost;
            _playerCheck.PlayerEnteredRanged -= OnPlayerEnteredRanged;
            _playerCheck.PlayerEnteredMelee -= OnPlayerEnteredMelee;
            _playerCheck.PlayerLeftRanged -= OnPlayerLeftRanged;
            _playerCheck.PlayerLeftMelee -= OnPlayerLeftMelee;
        }
    }

    public void ChangeState(EnemyState state)
    {
        if (_state == state) return;
        
        _state = state;
        
        var movable = GetComponent<IEnemyMovable>();
        if (movable != null)
        {
            if (state == EnemyState.Normal)
            {
                movable.EnableMovement(true);
                if (_playerCheck != null && _playerCheck.CurrentTarget != null)
                {
                    movable.SetTarget(_playerCheck.CurrentTarget);
                }
            }
            else if (state == EnemyState.Attack)
            {
                movable.EnableMovement(false);
            }
        }
    }

    public void AttackChoise()
    {
        if (_state != EnemyState.Attack || _playerCheck == null) return;

        var attacks = GetComponents<MonoBehaviour>();
        
        foreach (var component in attacks)
        {
            if (component is IAttack attack)
            {
                if (component is BasicEnemyAttackLogic basic)
                {
                    if (_playerCheck.IsInMelee && basic.Mode == BasicEnemyAttackDataSO.AttackMode.Melee)
                    {
                        attack.PerformAttack(Vector2.zero);
                        return;
                    }
                    if (_playerCheck.IsInRanged && !_playerCheck.IsInMelee && basic.Mode == BasicEnemyAttackDataSO.AttackMode.Ranged)
                    {
                        attack.PerformAttack(Vector2.zero);
                        return;
                    }
                }
            }
        }
    }

    public void AddEffect(EffectBase effect)
    {
        if (effect != null && !_activeEffects.Contains(effect))
        {
            _activeEffects.Add(effect);
            effect.ApplyEffect(gameObject);
        }
    }

    public void RemoveEffect(EffectBase effect)
    {
        if (effect != null && _activeEffects.Contains(effect))
        {
            _activeEffects.Remove(effect);
            effect.RemoveEffect(gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    private void OnPlayerDetected(Transform target)
    {
        ChangeState(EnemyState.Normal);
        var movable = GetComponent<IEnemyMovable>();
        movable?.SetTarget(target);
    }

    private void OnPlayerLost()
    {
        ChangeState(EnemyState.Normal);
        var movable = GetComponent<IEnemyMovable>();
        movable?.ClearTarget();
    }

    private void OnPlayerEnteredRanged(Transform target)
    {
        ChangeState(EnemyState.Attack);
        AttackChoise();
    }

    private void OnPlayerLeftRanged()
    {
        if (!_playerCheck.IsInMelee)
        {
            ChangeState(EnemyState.Normal);
        }
    }

    private void OnPlayerEnteredMelee(Transform target)
    {
        ChangeState(EnemyState.Attack);
        AttackChoise();
    }

    private void OnPlayerLeftMelee()
    {
        if (_playerCheck.IsInRanged)
        {
            AttackChoise();
        }
        else
        {
            ChangeState(EnemyState.Normal);
        }
    }
}

public enum EnemyState { Normal, Attack }