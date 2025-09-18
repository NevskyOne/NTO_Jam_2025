using UnityEngine;
using Zenject;

public class Character : MonoBehaviour, IInteractable
{
    [SerializeField] private string _startNode;
    [SerializeField] private Transform _dialoguePosition;
    [SerializeField] private Material _outlineMaterial;

    private DialogueStartSystem _dialogueSys;

    [Inject]
    private void Construct(DialogueStartSystem dialogueSys)
    {
        _dialogueSys = dialogueSys;
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
        UnmarkInteractable();
        _dialogueSys.StartDialogue(_startNode, _dialoguePosition);
    }

}
 
