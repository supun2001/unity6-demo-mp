using UnityEngine;
using PlayerCharacterController;
using System;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    [Header("Base Movement")]
    public float runAcceleration = 0.25f;
    public float runSpeed = 4f;
    public float drag = 0.1f;
    public float gravity = 25f;
    public float jumpForce = 1f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    private float _verticalVelocity = 0f;
    private Vector3 _horizontalVelocity = Vector3.zero;

    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        HandleVerticalMovement();
        HandleHorizontalMovement();

        // Combine horizontal and vertical velocity
        Vector3 finalVelocity = _horizontalVelocity;
        finalVelocity.y = _verticalVelocity;

        // Move the character
        _characterController.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleHorizontalMovement() {
        // Get camera-relative directions
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        
        // MovementInput.y = forward/back (W/S), MovementInput.x = left/right (A/D)
        Vector3 movementDirection = cameraForwardXZ * _playerLocomotionInput.MovementInput.y + cameraRightXZ * _playerLocomotionInput.MovementInput.x;

        // Apply acceleration
        Vector3 movementDelta = movementDirection * runAcceleration * Time.deltaTime;
        _horizontalVelocity += movementDelta;

        // Apply drag
        Vector3 currentDrag = _horizontalVelocity.normalized * drag * Time.deltaTime;
        _horizontalVelocity = (_horizontalVelocity.magnitude > drag * Time.deltaTime) ? _horizontalVelocity - currentDrag : Vector3.zero;
        
        // Clamp to max speed
        _horizontalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, runSpeed);
        
        // Keep it horizontal (zero out Y component)
        _horizontalVelocity.y = 0f;
    }

    private void LateUpdate() {
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y + lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);
        
        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;
        transform.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
    }

    public bool IsGrounded() {
        return _characterController.isGrounded;   
    }

    private void HandleVerticalMovement()
    {
        if (IsGrounded() && _verticalVelocity < 0){
            _verticalVelocity = 0f;
        }
        _verticalVelocity -= gravity * Time.deltaTime;

        if(_playerLocomotionInput.JumpPressed && IsGrounded()){
            _verticalVelocity += MathF.Sqrt(jumpForce * 3 * gravity);
        }
        
    }


}
