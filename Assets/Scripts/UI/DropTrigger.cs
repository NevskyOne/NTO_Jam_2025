
using UnityEngine;
using Zenject;

public class DropTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _visual;
    [SerializeField] private int _abilitySlot;
    private Player _player;
    private DragSystem _dragSystem;

    [Inject]
    private void Construct(Player player, DragSystem dragSystem)
    {
        _player = player;
        _dragSystem = dragSystem;
    }
    
    public void MarkDroppable(bool state)
    {
        _visual.SetActive(state);
        _dragSystem.OverUIElement = state ? this : null;
    }
    
    public bool OnDrop(Transform obj)
    {
        var food = obj.GetComponent<FoodUI>();
        if (food.State == FoodState.Shop && _player.TryBuy(food.Price))
        {
            _player.AddAbility(food);
        }
        else if (food.State == FoodState.Abilities && _abilitySlot > -1)
        {
            _player.SwitchAbility(_player.Data.UsedFood.IndexOf(food.Id), _abilitySlot);
        }
        else if (food.State == FoodState.Abilities && _abilitySlot == -1)
        {
            _player.AbilityToInventory(food);
        }
        else if (food.State == FoodState.Inventory)
        {
            if(transform.GetChild(0).TryGetComponent<FoodUI>(out var childAbility))
            {
                childAbility.State = FoodState.Inventory;
                childAbility.transform.SetParent(GameObject.FindWithTag("Inventory").transform);
            }
            _player.AbilityToMain(food, _abilitySlot);
        }
        else
        {
            return false;
        }

        return true;
    }
}
