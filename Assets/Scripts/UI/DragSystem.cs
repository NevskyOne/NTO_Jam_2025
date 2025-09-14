using UnityEngine.InputSystem;
using UnityEngine;
using Zenject;


public class DragSystem : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Vector2 _initPos;
    public Transform OverUIElement { get; set; }
    
    [Inject]
    private void Construct(PlayerInput input)
    {
        _playerInput = input;
    }

    public void GrabObj(Transform obj)
    {
        _initPos = obj.position;
    }
    
    public void MoveObj(Transform obj)
    {
        obj.position = _playerInput.actions["MousePosition"].ReadValue<Vector2>();
    }
    
    public void DropObj(Transform obj)
    {
        if (OverUIElement)
        {
            obj.SetParent(OverUIElement);
        }
        else
        {
            obj.position = _initPos;
        }
    }
}
