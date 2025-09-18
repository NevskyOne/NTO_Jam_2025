using UnityEngine;
using Zenject;

public class Character : MonoBehaviour, IInteractable
{
    [SerializeField] private string _startNode;
    [SerializeField] private Transform _dialoguePosition;
    [SerializeField] private GameObject _outLine;

    private DialogueStartSystem _dialogueSys;

    [Inject]
    private void Construct(DialogueStartSystem dialogueSys)
    {
        _dialogueSys = dialogueSys;
    }

    public void MarkInteractable()
    {
        _outLine.SetActive(true);
    }
    
    public void UnmarkInteractable()
    {
        _outLine.SetActive(false);
    }

    public void Interact()
    {
        UnmarkInteractable();
        _dialogueSys.StartDialogue(_startNode, _dialoguePosition);
    }

}
 
