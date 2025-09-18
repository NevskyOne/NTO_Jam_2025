
using UnityEngine;

public class Plant : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _ending;
    [SerializeField] private Material _outlineMaterial;
        
    private Player _player;
    
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
        _ending.SetActive(true);
    }
}
