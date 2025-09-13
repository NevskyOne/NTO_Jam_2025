using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов, которые могут быть анимированы
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        /// Воспроизведение анимации движения
        /// </summary>
        /// <param name="direction">Направление движения</param>
        /// <param name="speed">Скорость движения</param>
        void PlayMoveAnimation(Vector2 direction, float speed);
        
        /// <summary>
        /// Воспроизведение анимации прыжка
        /// </summary>
        void PlayJumpAnimation();
        
        /// <summary>
        /// Воспроизведение анимации падения
        /// </summary>
        void PlayFallAnimation();
        
        /// <summary>
        /// Воспроизведение анимации приземления
        /// </summary>
        void PlayLandAnimation();
        
        /// <summary>
        /// Воспроизведение анимации рывка (деша)
        /// </summary>
        /// <param name="direction">Направление рывка</param>
        void PlayDashAnimation(Vector2 direction);
        
        /// <summary>
        /// Воспроизведение анимации атаки
        /// </summary>
        /// <param name="attackType">Тип атаки (0 - обычная, 1 - с воздуха, и т.д.)</param>
        void PlayAttackAnimation(int attackType);
        
        /// <summary>
        /// Воспроизведение анимации парирования
        /// </summary>
        void PlayParryAnimation();
        
        /// <summary>
        /// Воспроизведение анимации получения урона
        /// </summary>
        void PlayHitAnimation();
        
        /// <summary>
        /// Воспроизведение анимации смерти
        /// </summary>
        void PlayDeathAnimation();
        
        /// <summary>
        /// Воспроизведение анимации использования еды/способности
        /// </summary>
        /// <param name="foodType">Тип еды/способности</param>
        void PlayFoodAnimation(int foodType);
        
        /// <summary>
        /// Проверка, закончилась ли текущая анимация
        /// </summary>
        /// <returns>true, если анимация закончилась, иначе false</returns>
        bool IsAnimationFinished();
    }
}
