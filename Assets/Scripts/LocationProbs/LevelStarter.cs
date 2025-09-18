using UnityEngine;
using Zenject;

public class LevelStarter : MonoBehaviour, IInteractable
{
    [SerializeField] private Material _outlineMaterial;
    [SerializeField]
    private LevelSystem _lvlSystem;
        
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
        _lvlSystem.LoadLevel();
    }
}
