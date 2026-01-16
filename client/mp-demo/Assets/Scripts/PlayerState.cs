using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;

    public enum PlayerMovementState
    {
        Idling=0,
        Walking=1,
        Sprinting=2,
        Dead=3,
    }
}
