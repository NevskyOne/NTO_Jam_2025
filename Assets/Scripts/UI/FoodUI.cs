using Core.Interfaces;
using UnityEngine;
using Zenject;

public class FoodUI : MonoBehaviour
{
    [field: SerializeReference] public int Id {get; private set;}
	[field: SerializeReference] public int Price {get; private set;}
	[field: SerializeReference, SubclassSelector] public IAttack Food {get; private set;}

	public FoodState State {get; set;} = FoodState.Shop;

	private DragSystem _dragSystem;

	[Inject]
    private void Construct(DragSystem dragSys)
    {
        _dragSystem = dragSys;
    }

	public void OnDragBegin(){
		print("begin");
		_dragSystem.GrabObj(transform);
	}

	public void OnDrag(){
		_dragSystem.MoveObj(transform);
	}

	public void OnDragEnd(){
		print("end");
		_dragSystem.DropObj(transform);
	}
}

public enum FoodState
{
	Inventory,
	Abilities,
	Shop
}
