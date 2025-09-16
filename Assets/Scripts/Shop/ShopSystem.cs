
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ShopSystem
{
	private List<int> _defaultFood = new(){0, 1, 2, 3, 4};
	private Player _player;
	private ShopUI _shopUI;

	[Inject]
    private void Construct(Player player, ShopUI shop)
    {
        _player = player;
        _shopUI = shop;
        foreach (var food in _defaultFood.Where(x => !_player.Data.InventoryFood.Contains(x) && !_player.Data.InventoryFood.Contains(x)))
        {
	        _shopUI.SpawnFood(food);
        }
    }

    public void BuyFood(FoodUI food){
	    if (_player.TryBuy(food.Price))
	    {
		    
	    }
	}
}
