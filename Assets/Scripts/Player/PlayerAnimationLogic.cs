using UnityEngine;

public class PlayerAnimationLogic
{
    // Константы для имен анимаций
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_RUN = "Run";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_FALL = "Fall";
    private const string ANIM_LAND = "Land";
    private const string ANIM_DASH = "Dash";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_AIR_ATTACK = "AirAttack";
    private const string ANIM_PARRY = "Parry";
    private const string ANIM_HIT = "Hit";
    private const string ANIM_DEATH = "Death";
    private const string ANIM_FOOD = "Food";
    
    // Аниматор (будет установлен позже)
    private Animator _animator;
    
    // Текущее состояние анимации
    private string _currentAnimationState = ANIM_IDLE;
    
    // Флаг завершения анимации
    private bool _isAnimationFinished = true;
    
    // Флаг для контроля вывода отладочных сообщений
    private bool _debugEnabled = false;
    
    // Предыдущие значения для предотвращения повторных логов
    private Vector2 _lastMoveDirection;
    private float _lastMoveSpeed;
    
    /// <summary>
    /// Установка аниматора
    /// </summary>
    /// <param name="animator">Компонент аниматора</param>
    public void SetAnimator(Animator animator)
    {
        _animator = animator;
    }
    
    /// <summary>
    /// Включение/выключение отладочных сообщений
    /// </summary>
    public void SetDebugEnabled(bool enabled)
    {
        _debugEnabled = enabled;
    }
    
    /// <summary>
    /// Воспроизведение анимации движения
    /// </summary>
    /// <param name="direction">Направление движения</param>
    /// <param name="speed">Скорость движения</param>
    public void PlayMoveAnimation(Vector2 direction, float speed)
    {
        // Проверяем, изменились ли параметры движения существенно
        bool significantChange = 
            Mathf.Abs(_lastMoveSpeed - speed) > 0.1f || 
            Vector2.Distance(_lastMoveDirection, direction) > 0.1f;
        
        // Сохраняем текущие значения
        _lastMoveDirection = direction;
        _lastMoveSpeed = speed;
        
        if (speed > 0.1f)
        {
            ChangeAnimationState(ANIM_RUN);
            if (_debugEnabled && significantChange)
            {
                Debug.Log($"Playing run animation, direction: {direction}, speed: {speed}");
            }
        }
        else
        {
            ChangeAnimationState(ANIM_IDLE);
            // Отключаем вывод для idle анимации, так как она вызывается очень часто
            // if (_debugEnabled && significantChange)
            // {
            //     Debug.Log("Playing idle animation");
            // }
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации прыжка
    /// </summary>
    public void PlayJumpAnimation()
    {
        ChangeAnimationState(ANIM_JUMP);
        if (_debugEnabled)
        {
            Debug.Log("Playing jump animation");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации падения
    /// </summary>
    public void PlayFallAnimation()
    {
        ChangeAnimationState(ANIM_FALL);
        if (_debugEnabled)
        {
            Debug.Log("Playing fall animation");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации приземления
    /// </summary>
    public void PlayLandAnimation()
    {
        ChangeAnimationState(ANIM_LAND);
        if (_debugEnabled)
        {
            Debug.Log("Playing land animation");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации рывка (деша)
    /// </summary>
    /// <param name="direction">Направление рывка</param>
    public void PlayDashAnimation(Vector2 direction)
    {
        ChangeAnimationState(ANIM_DASH);
        if (_debugEnabled)
        {
            Debug.Log($"Playing dash animation in direction: {direction}");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации атаки
    /// </summary>
    /// <param name="attackType">Тип атаки (0 - обычная, 1 - с воздуха, и т.д.)</param>
    public void PlayAttackAnimation(int attackType)
    {
        if (attackType == 0)
        {
            ChangeAnimationState(ANIM_ATTACK);
            if (_debugEnabled)
            {
                Debug.Log("Playing attack animation");
            }
        }
        else if (attackType == 1)
        {
            ChangeAnimationState(ANIM_AIR_ATTACK);
            if (_debugEnabled)
            {
                Debug.Log("Playing air attack animation");
            }
        }
        else
        {
            Debug.LogWarning($"Unknown attack type: {attackType}");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации парирования
    /// </summary>
    public void PlayParryAnimation()
    {
        ChangeAnimationState(ANIM_PARRY);
        if (_debugEnabled)
        {
            Debug.Log("Playing parry animation");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации получения урона
    /// </summary>
    public void PlayHitAnimation()
    {
        ChangeAnimationState(ANIM_HIT);
        if (_debugEnabled)
        {
            Debug.Log("Playing hit animation");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации смерти
    /// </summary>
    public void PlayDeathAnimation()
    {
        ChangeAnimationState(ANIM_DEATH);
        if (_debugEnabled)
        {
            Debug.Log("Playing death animation");
        }
    }
    
    /// <summary>
    /// Воспроизведение анимации использования еды/способности
    /// </summary>
    /// <param name="foodType">Тип еды/способности</param>
    public void PlayFoodAnimation(int foodType)
    {
        ChangeAnimationState(ANIM_FOOD);
        if (_debugEnabled)
        {
            Debug.Log($"Playing food animation for food type: {foodType}");
        }
    }
    
    /// <summary>
    /// Проверка, закончилась ли текущая анимация
    /// </summary>
    /// <returns>true, если анимация закончилась, иначе false</returns>
    public bool IsAnimationFinished()
    {
        return _isAnimationFinished;
    }
    
    /// <summary>
    /// Изменение состояния анимации
    /// </summary>
    /// <param name="newState">Новое состояние</param>
    private void ChangeAnimationState(string newState)
    {
        // Если пытаемся воспроизвести ту же анимацию, ничего не делаем
        if (_currentAnimationState == newState) return;
        
        // Устанавливаем новое состояние
        _currentAnimationState = newState;
        
        // Если аниматор установлен, воспроизводим анимацию
        if (_animator != null)
        {
            _animator.Play(newState);
        }
        
        // Сбрасываем флаг завершения анимации
        _isAnimationFinished = false;
        
        // В реальной реализации здесь будет логика для отслеживания завершения анимации
        // Например, через события анимации или таймеры
        
        // Для простоты сразу устанавливаем флаг завершения для некоторых анимаций
        if (newState == ANIM_IDLE || newState == ANIM_RUN)
        {
            _isAnimationFinished = true;
        }
    }
    
    /// <summary>
    /// Обработчик события завершения анимации (будет вызываться из аниматора)
    /// </summary>
    public void OnAnimationComplete()
    {
        _isAnimationFinished = true;
        if (_debugEnabled)
        {
            Debug.Log($"Animation {_currentAnimationState} completed");
        }
        
        // Возвращаемся к состоянию покоя после завершения некоторых анимаций
        if (_currentAnimationState == ANIM_ATTACK || 
            _currentAnimationState == ANIM_AIR_ATTACK || 
            _currentAnimationState == ANIM_PARRY ||
            _currentAnimationState == ANIM_HIT ||
            _currentAnimationState == ANIM_LAND ||
            _currentAnimationState == ANIM_FOOD)
        {
            ChangeAnimationState(ANIM_IDLE);
        }
    }
}
