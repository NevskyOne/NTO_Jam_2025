using System;
using UnityEngine;

public class InteractableTrigger : MonoBehaviour
{
    public Action<IInteractable> FindInter;
    public Action LostInter;
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(typeof(IInteractable), out var inter))
        {
            ((IInteractable)inter).MarkInteractable();
            FindInter?.Invoke((IInteractable)inter);
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(typeof(IInteractable), out var inter))
        {
            ((IInteractable)inter).UnmarkInteractable();
            LostInter?.Invoke();
        }
    }
}
