using System.Collections;
using UnityEngine;
using Core.Interfaces;

// Дополнительное поведение для зомби-киборга, работает ВМЕСТЕ с EnemyAI
public class CyborgZombieBehavior : MonoBehaviour
{
    [Header("Cyborg Zombie Settings")]
    [SerializeField] private float _backstabAngle = 45f;
    [SerializeField] private int _backstabDamage = 999;
    [SerializeField] private int _frontDamage = 2;
    [SerializeField] private float _contactCooldown = 1f;
    
    private EnemyAI _enemyAI;
    private PlayerCheckSystem _playerCheck;
    private BasicEnemyAttackLogic _basicAttack;
    private float _lastAttackTime;
    
    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
        _playerCheck = GetComponent<PlayerCheckSystem>();
        _basicAttack = GetComponent<BasicEnemyAttackLogic>();
        
        // Настраиваем как медленного наземного врага
        var movement = GetComponent<BasicEnemyMovementLogic>();
        if (movement != null)
        {
            movement.SetCanFly(false);
            movement.SetMinDistanceToTarget(0.8f);
        }
    }
    
    private void OnEnable()
    {
        // Подписываемся на события ПОСЛЕ EnemyAI
        if (_playerCheck != null)
        {
            _playerCheck.PlayerEnteredMelee += OnPlayerInMelee;
        }
    }
    
    private void OnDisable()
    {
        if (_playerCheck != null)
        {
            _playerCheck.PlayerEnteredMelee -= OnPlayerInMelee;
        }
    }
    
    private void OnPlayerInMelee(Transform player)
    {
        // Проверяем кулдаун
        if (Time.time - _lastAttackTime < _contactCooldown) return;
        
        PerformBackstabAttack(player);
        _lastAttackTime = Time.time;
    }
    
    private void PerformBackstabAttack(Transform player)
    {
        if (player == null) return;
        
        // Определяем направление атаки
        Vector2 toPlayer = (player.position - transform.position).normalized;
        Vector2 playerFacing = GetPlayerFacingDirection(player);
        
        // Вычисляем угол между направлением к игроку и направлением взгляда игрока
        float angle = Vector2.Angle(-playerFacing, toPlayer);
        bool isBackstab = angle <= _backstabAngle;
        
        // Наносим урон
        var hittable = player.GetComponent<IHittable>();
        if (hittable != null)
        {
            int damage = isBackstab ? _backstabDamage : _frontDamage;
            hittable.TakeDamage(damage);
            
            if (isBackstab)
            {
                Debug.Log("💀 BACKSTAB! Зомби-киборг нанес критический урон!");
                StartCoroutine(BackstabEffect());
            }
            else
            {
                Debug.Log("🧟 Зомби-киборг атакует спереди");
            }
        }
    }
    
    private Vector2 GetPlayerFacingDirection(Transform player)
    {
        var playerMovement = player.GetComponent<PlayerMovementLogic>();
        if (playerMovement != null)
        {
            return playerMovement.LastDirection;
        }
        
        var spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        
        return Vector2.right;
    }
    
    private IEnumerator BackstabEffect()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }
    
    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        if (_playerCheck != null && _playerCheck.CurrentTarget != null)
        {
            Vector2 toPlayer = (_playerCheck.CurrentTarget.position - transform.position).normalized;
            Vector2 playerFacing = GetPlayerFacingDirection(_playerCheck.CurrentTarget);
            
            float angle = Vector2.Angle(-playerFacing, toPlayer);
            bool isBackstab = angle <= _backstabAngle;
            
            Gizmos.color = isBackstab ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position, _playerCheck.CurrentTarget.position);
            
            // Показываем конус ваншота
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Vector3 playerPos = _playerCheck.CurrentTarget.position;
            Vector3 left = Quaternion.Euler(0, 0, _backstabAngle) * (-playerFacing);
            Vector3 right = Quaternion.Euler(0, 0, -_backstabAngle) * (-playerFacing);
            
            Gizmos.DrawLine(playerPos, playerPos + left * 2f);
            Gizmos.DrawLine(playerPos, playerPos + right * 2f);
        }
    }
}
