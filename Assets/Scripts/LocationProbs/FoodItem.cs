using UnityEngine;
using Zenject;

public class FoodItem : MonoBehaviour, IInteractable
{
    [SerializeField] private FoodUI _foodUI;
    [SerializeField] private Material _outlineMaterial;

    private Player _player;

    [Inject]
    private void Construct(Player player)
    {
        _player = player;
    }
    public void MarkInteractable()
    {
        _outlineMaterial.SetFloat("_OutlineEnabled", 1);
    }
    
    public void UnmarkInteractable()
    {
        _outlineMaterial.SetFloat("_OutlineEnabled", 0);
    }

    public void Interact()
    {
        _player.AddAbility(_foodUI);
        Destroy(gameObject);
    }
}