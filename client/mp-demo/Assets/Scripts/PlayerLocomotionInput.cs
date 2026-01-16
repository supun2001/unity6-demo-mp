using UnityEngine;
using UnityEngine.InputSystem;
using PlayerCharacterController;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour,
    PlayerControls.IPlayerLocomotionMapActions
{
    public PlayerControls Controls { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    private void OnEnable()
    {
        Controls = new PlayerControls();

        Controls.PlayerLocomotionMap.SetCallbacks(this);
        Controls.PlayerLocomotionMap.Enable();
    }

    private void OnDisable()
    {
        Controls.PlayerLocomotionMap.RemoveCallbacks(this);
        Controls.PlayerLocomotionMap.Disable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
        Debug.Log($"Movement: {MovementInput}");
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
        Debug.Log($"Look: {LookInput}");
    }
}
