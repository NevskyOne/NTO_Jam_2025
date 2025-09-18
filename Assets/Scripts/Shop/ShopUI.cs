using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _pricePrefab;
    [SerializeField] private Transform _foodLayoutGroup;
    [SerializeField] private Transform _priceLayoutGroup;
    [SerializeField] private List<FoodUI> _defaultFoods;

     private DiContainer _diContainer;
     private ShopSystem _shopSystem;

    [Inject]
    public void Construct(DiContainer container, ShopSystem system)
    {
        _diContainer = container;
        _shopSystem = system;
    }

    public void Awake()
    {
        _shopSystem.SpawnFood();
    }
    
    public void SpawnFood(int food)
    {
        // Проверка границ массива для предотвращения ArgumentOutOfRangeException
        if (_defaultFoods == null || food < 0 || food >= _defaultFoods.Count)
        {
            Debug.LogWarning($"[ShopUI] Попытка создать еду с индексом {food}, но доступны индексы 0-{(_defaultFoods?.Count - 1 ?? -1)}");
            return;
        }

        var foodObj = _diContainer.InstantiatePrefab(_defaultFoods[food].gameObject, _foodLayoutGroup);
        Instantiate(_pricePrefab, _priceLayoutGroup);
        _pricePrefab.GetComponentInChildren<TMP_Text>().text = _defaultFoods[food].Price.ToString();
    }

    public void DeletePrice(int i)
    {
        // Проверка границ для предотвращения ошибок
        if (_priceLayoutGroup == null || i < 0 || i >= _priceLayoutGroup.childCount)
        {
            Debug.LogWarning($"[ShopUI] Попытка удалить цену с индексом {i}, но доступны индексы 0-{(_priceLayoutGroup?.childCount - 1 ?? -1)}");
            return;
        }
        Destroy(_priceLayoutGroup.GetChild(i));
    }
}
