using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float rotationSpeed = 10f; 
    [SerializeField] private float animationSmoothTime = 0.1f; 
    
    private PlayerLocomotionInput _playerLocomotionInput;

    private static int _inputXHash = Animator.StringToHash("inputX");
    private static int _inputYHash = Animator.StringToHash("inputY");

    private float _currentInputX = 0f;
    private float _currentInputY = 0f;

    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
    
    }

    private void Update() {
        UpdateAnimation();
    }

    private void UpdateAnimation() {
        Vector2 input = _playerLocomotionInput.MovementInput;

        // Smoothly interpolate animation values
        float smoothSpeed = animationSmoothTime > 0 ? 1f / animationSmoothTime : 10f;
        _currentInputX = Mathf.Lerp(_currentInputX, input.x, smoothSpeed * Time.deltaTime);
        _currentInputY = Mathf.Lerp(_currentInputY, input.y, smoothSpeed * Time.deltaTime);

        // Set smoothed values to animator
        _animator.SetFloat(_inputXHash, _currentInputX);
        _animator.SetFloat(_inputYHash, _currentInputY);
    }
}
