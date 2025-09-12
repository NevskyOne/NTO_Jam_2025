using UnityEngine;
using System.Collections.Generic;

public class SomeFoodLogic
{
    // Типы еды
    public enum FoodType
    {
        Tea = 0,            // Чай
        IceLatte = 1,       // Айс латте
        DragonFruit = 2,    // Драконий фрукт
        Dumplings = 3,      // Пельмени
        KoreanCarrot = 4,   // Корейская морковка
        Ratatouille = 5,    // Рататуй
        Burger = 6,         // Бургер
        ExplosiveCaramel = 7, // Взрывная карамель
        PoisonousPotato = 8 // Ядовитая картошка
    }
    
    // Словарь для хранения активных эффектов еды и их длительности
    private Dictionary<FoodType, float> _activeEffects = new Dictionary<FoodType, float>();
    
    // Префабы для визуальных эффектов и снарядов (в реальной реализации будут загружаться из ресурсов)
    private Dictionary<FoodType, GameObject> _foodPrefabs = new Dictionary<FoodType, GameObject>();
    
    /// <summary>
    /// Использование еды/способности
    /// </summary>
    /// <param name="foodType">Тип еды</param>
    /// <param name="position">Позиция игрока</param>
    /// <param name="direction">Направление использования</param>
    public void UseFood(int foodTypeInt, Vector3 position, Vector2 direction)
    {
        FoodType foodType = (FoodType)foodTypeInt;
        
        switch (foodType)
        {
            case FoodType.Tea:
                UseTea(position, direction);
                break;
            case FoodType.IceLatte:
                UseIceLatte(position, direction);
                break;
            case FoodType.DragonFruit:
                UseDragonFruit(position);
                break;
            case FoodType.Dumplings:
                UseDumplings(position);
                break;
            case FoodType.KoreanCarrot:
                UseKoreanCarrot(position);
                break;
            case FoodType.Ratatouille:
                UseRatatouille(position, direction);
                break;
            case FoodType.Burger:
                UseBurger(position);
                break;
            case FoodType.ExplosiveCaramel:
                UseExplosiveCaramel(position);
                break;
            case FoodType.PoisonousPotato:
                UsePoisonousPotato(position, direction);
                break;
            default:
                Debug.LogWarning($"Unknown food type: {foodType}");
                break;
        }
        
        Debug.Log($"Used food: {foodType} at position {position} in direction {direction}");
    }
    
    /// <summary>
    /// Обновление активных эффектов еды
    /// </summary>
    /// <param name="deltaTime">Время между кадрами</param>
    public void UpdateFoodEffects(float deltaTime)
    {
        List<FoodType> effectsToRemove = new List<FoodType>();
        
        foreach (var effect in _activeEffects)
        {
            _activeEffects[effect.Key] -= deltaTime;
            
            if (_activeEffects[effect.Key] <= 0)
            {
                effectsToRemove.Add(effect.Key);
            }
        }
        
        foreach (var effect in effectsToRemove)
        {
            _activeEffects.Remove(effect);
            OnFoodEffectEnd(effect);
        }
    }
    
    /// <summary>
    /// Проверка, активен ли эффект еды
    /// </summary>
    /// <param name="foodType">Тип еды</param>
    /// <returns>true, если эффект активен, иначе false</returns>
    public bool IsFoodEffectActive(FoodType foodType)
    {
        return _activeEffects.ContainsKey(foodType) && _activeEffects[foodType] > 0;
    }
    
    /// <summary>
    /// Получение оставшегося времени действия эффекта
    /// </summary>
    /// <param name="foodType">Тип еды</param>
    /// <returns>Оставшееся время в секундах или 0, если эффект не активен</returns>
    public float GetRemainingEffectTime(FoodType foodType)
    {
        if (_activeEffects.ContainsKey(foodType))
        {
            return _activeEffects[foodType];
        }
        return 0f;
    }
    
    #region Food Implementations
    
    // Чай - облитие противников кипятком (наносит n урона каждый ход, 2x на лед)
    private void UseTea(Vector3 position, Vector2 direction)
    {
        // Создание области поражения перед игроком
        Vector2 spawnPosition = (Vector2)position + direction * 1.5f;
        
        // В реальной реализации здесь будет создание объекта с эффектом кипятка
        Debug.Log($"Tea: Hot water splash at {spawnPosition}");
        
        // Добавление эффекта в активные с длительностью 5 секунд
        _activeEffects[FoodType.Tea] = 5f;
    }
    
    // Айс латте - ледяные осколки + медлительность (2x на огонь)
    private void UseIceLatte(Vector3 position, Vector2 direction)
    {
        // Создание ледяных осколков в направлении курсора
        Vector2 spawnPosition = (Vector2)position;
        
        // В реальной реализации здесь будет создание снарядов-осколков
        Debug.Log($"Ice Latte: Ice shards at {spawnPosition} in direction {direction}");
        
        // Добавление эффекта в активные с длительностью 3 секунды
        _activeEffects[FoodType.IceLatte] = 3f;
    }
    
    // Драконий фрукт - щит с шипами
    private void UseDragonFruit(Vector3 position)
    {
        // Создание щита вокруг игрока
        
        // В реальной реализации здесь будет создание объекта-щита
        Debug.Log($"Dragon Fruit: Shield activated at {position}");
        
        // Добавление эффекта в активные с длительностью 10 секунд
        _activeEffects[FoodType.DragonFruit] = 10f;
    }
    
    // Пельмени - липкие ловушки-тесто
    private void UseDumplings(Vector3 position)
    {
        // Создание ловушки под игроком
        
        // В реальной реализации здесь будет создание объекта-ловушки
        Debug.Log($"Dumplings: Trap placed at {position}");
    }
    
    // Корейская морковка - дабл джамп + враги на пару секунд в воздухе
    private void UseKoreanCarrot(Vector3 position)
    {
        // Активация эффекта двойного прыжка
        
        Debug.Log($"Korean Carrot: Double jump activated at {position}");
        
        // Добавление эффекта в активные с длительностью 15 секунд
        _activeEffects[FoodType.KoreanCarrot] = 15f;
    }
    
    // Рататуй - 3 тентакли по направлению курсора (2x урон по жиру)
    private void UseRatatouille(Vector3 position, Vector2 direction)
    {
        // Создание тентаклей в направлении курсора
        
        // В реальной реализации здесь будет создание объектов-тентаклей
        Debug.Log($"Ratatouille: Tentacles at {position} in direction {direction}");
    }
    
    // Бургер - руки в жире, накладывает жирность на врага
    private void UseBurger(Vector3 position)
    {
        // Активация эффекта жирных рук
        
        Debug.Log($"Burger: Greasy hands activated at {position}");
        
        // Добавление эффекта в активные с длительностью 20 секунд
        _activeEffects[FoodType.Burger] = 20f;
    }
    
    // Взрывная карамель - бомба с радиусом поражения
    private void UseExplosiveCaramel(Vector3 position)
    {
        // Создание бомбы на месте игрока
        
        // В реальной реализации здесь будет создание объекта-бомбы
        Debug.Log($"Explosive Caramel: Bomb placed at {position}");
    }
    
    // Ядовитая картошка - волна картошки из-под земли
    private void UsePoisonousPotato(Vector3 position, Vector2 direction)
    {
        // Создание волны картошки в направлении движения
        
        // В реальной реализации здесь будет создание объекта-волны
        Debug.Log($"Poisonous Potato: Wave at {position} in direction {direction}");
    }
    
    #endregion
    
    /// <summary>
    /// Обработка окончания действия эффекта еды
    /// </summary>
    /// <param name="foodType">Тип еды</param>
    private void OnFoodEffectEnd(FoodType foodType)
    {
        Debug.Log($"Food effect ended: {foodType}");
        
        // Здесь будет логика для обработки окончания эффектов
        // Например, удаление визуальных эффектов, сброс бонусов и т.д.
    }
}
