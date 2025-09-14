using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Core.Input
{
    public class PlayerInput : IInitializable, IDisposable
    {
        private InputActionAsset _inputActions;
        private InputActionMap _map;
        private InputAction _move;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _leftMouseAction;
        private InputAction _rightMouseAction;
        private InputAction _qAction;
        private InputAction _eAction;
        private InputAction _downAttackAction;

        // События для подписки
        public event Action<Vector2> OnMovePerformed;
        public event Action OnMoveCanceled;
        public event Action OnJumpPerformed;
        public event Action<Vector2> OnDashPerformed;
        public event Action<Vector2> OnLeftMousePerformed;
        public event Action OnRightMousePerformed;
        public event Action OnQPerformed;
        public event Action OnEPerformed;
        public event Action<Vector2> OnDownAttackPerformed;

        private Vector2 _cachedMove;

        [Inject]
        public void Construct(InputActionAsset inputActions)
        {
            _inputActions = inputActions;
        }

        public void Initialize()
        {
            if (_inputActions != null)
            {
                _map = _inputActions.FindActionMap("Player", throwIfNotFound: true);
                _move = _map.FindAction("Move", throwIfNotFound: true);
                _jumpAction = _map.FindAction("Jump", throwIfNotFound: true);
                _dashAction = _map.FindAction("Shift", throwIfNotFound: true);
                _leftMouseAction = _map.FindAction("LeftMouse", throwIfNotFound: true);
                _rightMouseAction = _map.FindAction("RightMouse", throwIfNotFound: true);
                _qAction = _map.FindAction("Q", throwIfNotFound: true);
                _eAction = _map.FindAction("E", throwIfNotFound: true);
                _downAttackAction = _map.FindAction("DownAttack", throwIfNotFound: false);

                // Подписки на события
                _move.performed += HandleMovePerformed;
                _move.canceled += HandleMoveCanceled;
                _jumpAction.performed += HandleJumpPerformed;
                _dashAction.performed += HandleDashPerformed;
                _leftMouseAction.performed += HandleLeftMousePerformed;
                _rightMouseAction.performed += HandleRightMousePerformed;
                _qAction.performed += HandleQPerformed;
                _eAction.performed += HandleEPerformed;
                if (_downAttackAction != null) _downAttackAction.performed += HandleDownAttackPerformed;

                _map.Enable();
            }
            else
            {
                Debug.LogError("PlayerInput: InputActionAsset не привязан");
            }
        }

        public void Dispose()
        {
            if (_map != null)
            {
                if (_move != null) { _move.performed -= HandleMovePerformed; _move.canceled -= HandleMoveCanceled; }
                if (_jumpAction != null) _jumpAction.performed -= HandleJumpPerformed;
                if (_dashAction != null) _dashAction.performed -= HandleDashPerformed;
                if (_leftMouseAction != null) _leftMouseAction.performed -= HandleLeftMousePerformed;
                if (_rightMouseAction != null) _rightMouseAction.performed -= HandleRightMousePerformed;
                if (_qAction != null) _qAction.performed -= HandleQPerformed;
                if (_eAction != null) _eAction.performed -= HandleEPerformed;
                if (_downAttackAction != null) _downAttackAction.performed -= HandleDownAttackPerformed;
                _map.Disable();
            }
        }

        private void HandleMovePerformed(InputAction.CallbackContext ctx)
        {
            try { _cachedMove = ctx.ReadValue<Vector2>(); }
            catch { float x = 0f; try { x = ctx.ReadValue<float>(); } catch { x = 0f; } _cachedMove = new Vector2(x, 0f); }
            OnMovePerformed?.Invoke(_cachedMove);
        }

        private void HandleMoveCanceled(InputAction.CallbackContext ctx)
        {
            _cachedMove = Vector2.zero;
            OnMoveCanceled?.Invoke();
        }

        private void HandleJumpPerformed(InputAction.CallbackContext ctx) => OnJumpPerformed?.Invoke();
        private void HandleDashPerformed(InputAction.CallbackContext ctx) => OnDashPerformed?.Invoke(_cachedMove.normalized);
        private void HandleLeftMousePerformed(InputAction.CallbackContext ctx) => OnLeftMousePerformed?.Invoke(_cachedMove);
        private void HandleRightMousePerformed(InputAction.CallbackContext ctx) => OnRightMousePerformed?.Invoke();
        private void HandleQPerformed(InputAction.CallbackContext ctx) => OnQPerformed?.Invoke();
        private void HandleEPerformed(InputAction.CallbackContext ctx) => OnEPerformed?.Invoke();
        private void HandleDownAttackPerformed(InputAction.CallbackContext ctx) => OnDownAttackPerformed?.Invoke(_cachedMove);

        public Vector2 GetCachedMove() => _cachedMove;

        public InputActionAsset GetInputActionAsset() => _inputActions;
    }
}
