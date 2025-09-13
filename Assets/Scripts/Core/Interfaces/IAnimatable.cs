using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов, которые могут быть анимированы
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        /// Воспроизведение анимации
        /// </summary>
        /// <param name="animationName">Имя анимации</param>
        void PlayAnimation(string animationName);
    }
}
