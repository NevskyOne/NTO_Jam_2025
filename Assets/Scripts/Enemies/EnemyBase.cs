using UnityEngine;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Data.ScriptableObjects;

public class EnemyBase : MonoBehaviour, IMovable, IHittable 
{
    [SerializeField] private List<IAttack> _attackSet;
    [SerializeField] public EnemyDataSO Data;
    private int _currentHealth;

    public void Move(Vector2 direction)
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;
        float speed = Data != null ? Data.MoveSpeed : 0f;
        Vector2 v = rb.linearVelocity;
        v.x = direction.normalized.x * speed;
        rb.linearVelocity = v;
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0)
        {
            _currentHealth = Data != null ? Data.MaxHealth : 1;
        }
        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.Max(0, amount));
        if (_currentHealth == 0)
        {
            Die();
        }
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(Mathf.CeilToInt(amount));
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}