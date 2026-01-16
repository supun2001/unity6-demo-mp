using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private PlayerLocomotionInput _playerLocomotionInput;

    private static int _inputXHash = Animator.StringToHash("inputX");
    private static int _inputYHash = Animator.StringToHash("inputY");

    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
    
    }

    private void Update() {
        UpdateAnimation();
    }

    private void UpdateAnimation() {
        Vector2 input = _playerLocomotionInput.MovementInput;

        _animator.SetFloat(_inputXHash, input.x);
        _animator.SetFloat(_inputYHash, input.y);
    }
}
