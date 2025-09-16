using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _pricePrefab;
    [SerializeField] private Transform _foodLayoutGroup;
    [SerializeField] private Transform _priceLayoutGroup;
    [SerializeField] private List<FoodUI> _defaultFoods;

    public void SpawnFood(int food)
    {
        Instantiate(_defaultFoods[food].gameObject, _foodLayoutGroup);
        Instantiate(_pricePrefab, _priceLayoutGroup);
        _pricePrefab.GetComponentInChildren<TMP_Text>().text = _defaultFoods[food].Price.ToString();
    }

    public void DeletePrice(int i)
    {
        Destroy(_priceLayoutGroup.GetChild(i));
    }
}
