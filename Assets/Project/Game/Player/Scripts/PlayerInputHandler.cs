using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Game.Player.Scripts
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MovementInput { get; private set; }
        public bool IsShooting { get; private set; }

        private InputSystem_Actions _controls;

        private System.Action<InputAction.CallbackContext> _movePerformedAction;
        private System.Action<InputAction.CallbackContext> _moveCanceledAction;
        private System.Action<InputAction.CallbackContext> _attackPerformedAction;

        private void Awake()
        {
            _controls = new InputSystem_Actions();

            _movePerformedAction = ctx => MovementInput = ctx.ReadValue<Vector2>();
            _moveCanceledAction = _ => MovementInput = Vector2.zero;
            _attackPerformedAction = _ => IsShooting = true;
        }

        private void OnEnable()
        {
            _controls.Enable();

            _controls.Player.Move.performed += _movePerformedAction;
            _controls.Player.Move.canceled += _moveCanceledAction;
            _controls.Player.Attack.performed += _attackPerformedAction;
        }

        private void OnDisable()
        {
            _controls.Player.Move.performed -= _movePerformedAction;
            _controls.Player.Move.canceled -= _moveCanceledAction;
            _controls.Player.Attack.performed -= _attackPerformedAction;

            _controls.Disable();
        }

        public void ResetShoot()
        {
            IsShooting = false;
        }
    }
}