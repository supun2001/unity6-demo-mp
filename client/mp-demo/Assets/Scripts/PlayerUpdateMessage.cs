using System;

[Serializable]
public class PlayerUpdateMessage
{
    public float x;
    public float y;
    public float z;
    public float rotationY;
    public float velocityX;
    public float velocityY;
    public float velocityZ;
    public float animInputX;
    public float animInputY;
    public bool isGrounded;
    public bool isJumping;
    public float cameraRotationX;
    public float cameraRotationY;
}
