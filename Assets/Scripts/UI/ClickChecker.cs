using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickChecker : MonoBehaviour, IPointerClickHandler
{
    private DialogueStartSystem _dialogueStartSystem;
    
    private void Start() => _dialogueStartSystem = FindFirstObjectByType<DialogueStartSystem>();
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _dialogueStartSystem.OnEndLine(new InputAction.CallbackContext());
    }
}
