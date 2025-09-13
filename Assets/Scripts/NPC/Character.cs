using UnityEngine;
using Zenject;

public class Character : MonoBehaviour, IInteractable
{
    [SerializeField] private string _startNode;
    [SerializeField] private Transform _dialoguePosition;

    private DialogueStartSystem _dialogueSys;

    [Inject]
    private void Construct(DialogueStartSystem dialogueSys)
    {
        _dialogueSys = dialogueSys;
    }
    
    public void Start() => Interact();
    
    public void Interact()
    {
        _dialogueSys.StartDialogue(_startNode, _dialoguePosition);
    }

}
 
