using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
[RequireComponent(typeof(EnemyAttack))]
public class EnemyAI : MonoBehaviour
{
    [Header("Distances")]
    [SerializeField] private float attackDistance = 1.0f; // дистанция, на которой начинаем атаку
    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.75f; // сек между ударами

    private EnemyBase _enemyBase;
    private EnemyAttack _enemyAttack;
    private Transform _player;
    private float _attackTimer;

    private void Awake()
    {
        _enemyBase = GetComponent<EnemyBase>();
        _enemyAttack = GetComponent<EnemyAttack>();
    }

    private void TryFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            _player = go.transform;
    }

    private void FixedUpdate()
    {
        if (_attackTimer > 0f)
            _attackTimer -= Time.fixedDeltaTime;

        if (_player == null)
            TryFindPlayer();
        if (_player == null)
            return;

        float dx = _player.position.x - transform.position.x;
        float absDx = Mathf.Abs(dx);

        if (absDx > attackDistance)
        {
            // Двигаемся по X в сторону игрока
            Vector2 dir = new Vector2(Mathf.Sign(dx), 0f);
            _enemyBase.Move(dir);
        }
        else
        {
            // Останавливаемся и атакуем по кулдауну
            _enemyBase.Move(Vector2.zero);
            if (_attackTimer <= 0f)
            {
                _enemyAttack.Attack();
                _attackTimer = attackCooldown;
            }
        }
    }
}
