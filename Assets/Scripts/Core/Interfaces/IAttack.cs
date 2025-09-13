using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов, которые могут атаковать
    /// </summary>
    public interface IAttack
    {
        /// <summary>
        /// Выполнение базовой атаки
        /// </summary>
        /// <param name="direction">Направление атаки</param>
        void PerformAttack(Vector2 direction);
        
        /// <summary>
        /// Выполнение атаки с воздуха
        /// </summary>
        /// <param name="direction">Направление атаки</param>
        void PerformAirAttack(Vector2 direction);
        
        /// <summary>
        /// Выполнение парирования
        /// </summary>
        void PerformParry();
        
        /// <summary>
        /// Проверка, находится ли атака в процессе выполнения
        /// </summary>
        /// <returns>true, если атака выполняется, иначе false</returns>
        bool IsAttacking();
        
        /// <summary>
        /// Проверка, находится ли парирование в процессе выполнения
        /// </summary>
        /// <returns>true, если парирование выполняется, иначе false</returns>
        bool IsParrying();
        
        /// <summary>
        /// Получение текущего урона атаки
        /// </summary>
        /// <returns>Значение урона</returns>
        float GetDamage();
        
        /// <summary>
        /// Получение текущего радиуса атаки
        /// </summary>
        /// <returns>Радиус атаки</returns>
        float GetAttackRadius();
    }
}
