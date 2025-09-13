using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private Player _player;
    private Vector2 _moveDirection;
    
    private bool _jumpPressed = false;
    private bool _dashPressed = false;
    private bool _attackPressed = false;
    private bool _parryPressed = false;
    private bool _anyKeyPressed = false;
    private bool _isInitialized = false;

    private void Awake()
    {
        _player = GetComponent<Player>();
        
        if (_player == null)
        {
            Debug.LogError("PlayerInputHandler: Player компонент не найден!");
            enabled = false;
        }
    }
    
    private void Start()
    {
        _isInitialized = true;
        
        if (_player != null)
        {
            _player.ResetAllStates();
        }
        
        Debug.Log("PlayerInputHandler started");
    }

    private void Update()
    {
        if (!_isInitialized || _player == null) return;
        
        _player.Move(_moveDirection, Time.deltaTime);
        ProcessKeyInput();
        
        _jumpPressed = false;
        _dashPressed = false;
        _attackPressed = false;
        _parryPressed = false;
    }
    
    private void ProcessKeyInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (!_anyKeyPressed)
        {
            if (keyboard.anyKey.isPressed || Mouse.current != null && (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed))
            {
                _anyKeyPressed = true;
                Debug.Log("First key press detected");
            }
        }
        
        Vector2 oldDirection = _moveDirection;
        _moveDirection = Vector2.zero;
        
        if (keyboard.aKey.isPressed) _moveDirection.x -= 1;
        if (keyboard.dKey.isPressed) _moveDirection.x += 1;
        
        if (oldDirection != _moveDirection)
        {
            Debug.Log($"Move direction changed: {oldDirection} -> {_moveDirection}");
        }
        
        if (keyboard.spaceKey.isPressed && !_jumpPressed)
        {
            _jumpPressed = true;
            Debug.Log("Jump key pressed");
            _player.Jump();
        }
        
        if (keyboard.leftShiftKey.isPressed && !_dashPressed)
        {
            _dashPressed = true;
            Debug.Log("Dash key pressed");
            _player.Dash(_moveDirection.normalized);
        }
        
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed && !_attackPressed)
        {
            _attackPressed = true;
            Debug.Log("Attack button pressed");
            _player.PerformAttack(_moveDirection);
        }
        
        if (mouse != null && mouse.rightButton.isPressed && !_parryPressed)
        {
            _parryPressed = true;
            Debug.Log("Parry button pressed");
            _player.PerformParry();
        }
        
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Food key pressed");
            _player.UseFood(0);
        }
    }
}
