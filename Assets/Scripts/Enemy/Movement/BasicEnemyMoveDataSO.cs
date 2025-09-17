using UnityEngine;

[CreateAssetMenu(fileName = "BasicEnemyMoveData", menuName = "Game Data/Enemy/Move Data")]
public class BasicEnemyMoveDataSO : ScriptableObject
{
    [Header("Движение")]
    [SerializeField] private float _moveSpeed = 2.5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 5f;
    
    [Header("Поворот")]
    [SerializeField] private bool _flipSprite = true;
    
    public float MoveSpeed => _moveSpeed;
    public float Acceleration => _acceleration;
    public float Deceleration => _deceleration;
    public bool FlipSprite => _flipSprite;
}
