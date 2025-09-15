using UnityEngine;
using Core.Interfaces;

[DisallowMultipleComponent]
public class EnemyAttack : MonoBehaviour
{ 
    public void Attack()
    
    {
        var enemy = GetComponent<EnemyBase>();
        if (enemy == null) return;

        int damage = enemy.Data != null ? enemy.Data.Damage : 1;

        // Радиус атаки исходя из размера коллайдера или дефолт
        float radius = 1.0f;
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            var ext = col.bounds.extents;
            radius = Mathf.Max(0.8f, Mathf.Max(ext.x, ext.y) + 0.2f);
        }

        Vector2 center = transform.position;
        var hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (var h in hits)
        {
            if (h.transform == transform) continue;
            var target = h.GetComponent<IHittable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                break; // Бьём первую подходящую цель
            }
        }
    }
}
