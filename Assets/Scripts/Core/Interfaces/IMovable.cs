using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов, которые могут двигаться
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Перемещение объекта в указанном направлении
        /// </summary>
        /// <param name="direction">Направление движения</param>
        /// <param name="deltaTime">Время между кадрами</param>
        void Move(Vector2 direction, float deltaTime);
        
        /// <summary>
        /// Выполнение прыжка
        /// </summary>
        void Jump();
        
        /// <summary>
        /// Выполнение рывка (деша) в указанном направлении
        /// </summary>
        /// <param name="direction">Направление рывка</param>
        void Dash(Vector2 direction);
        
        /// <summary>
        /// Проверка, находится ли объект на земле
        /// </summary>
        /// <returns>true, если объект на земле, иначе false</returns>
        bool IsGrounded();
        
        /// <summary>
        /// Получение текущей скорости объекта
        /// </summary>
        /// <returns>Текущая скорость в виде Vector2</returns>
        Vector2 GetVelocity();
        
        /// <summary>
        /// Установка скорости объекта
        /// </summary>
        /// <param name="velocity">Новая скорость</param>
        void SetVelocity(Vector2 velocity);
    }
}
