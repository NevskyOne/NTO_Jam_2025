using UnityEngine;
using Zenject;

public class HealItem : MonoBehaviour, IInteractable
{
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
        _player.AddPill();
        Destroy(gameObject);
    }
}
