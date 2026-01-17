using UnityEngine;
using Colyseus.Schema;
using System.Collections.Generic;

public class NetworkPlayer : MonoBehaviour
{
    private Player playerState;
    private bool isLocal;
    
    private Vector3 targetPos;
    private Quaternion targetRot;
    private float lerpSpeed = 15f;

    // References
    private PlayerController controller;
    private PlayerLocomotionInput input;
    private PlayerAnimation anim; 
    private Animator animator;

    public void Initialize(Player state, bool isLocalPlayer)
    {
        playerState = state;
        isLocal = isLocalPlayer;
        
        controller = GetComponent<PlayerController>();
        input = GetComponent<PlayerLocomotionInput>();
        anim = GetComponent<PlayerAnimation>();
        animator = GetComponent<Animator>();

        if (!isLocal)
        {
            if (controller) controller.enabled = false;
            
            if (input) 
            {
                input.enabled = false;
                if (input.Controls != null) input.Controls.Disable(); 
            }
        }
    }

    private float nextSendTime = 0f;
    public float sendInterval = 0.05f; // 20 times per second

    private void Update()
    {
        if (isLocal)
        {
            if (Time.time >= nextSendTime)
            {
                SendLocalState();
                nextSendTime = Time.time + sendInterval;
            }
        }
        else
        {
            UpdateRemoteState();
            InterpolateRemotePlayer();
        }
    }

    private void SendLocalState()
    {
        if (NetworkManager.Instance == null) return;

        float camRx = Camera.main ? Camera.main.transform.localEulerAngles.x : 0;
        float camRy = Camera.main ? Camera.main.transform.localEulerAngles.y : 0;

        float animX = animator ? animator.GetFloat("inputX") : 0; 
        float animY = animator ? animator.GetFloat("inputY") : 0;

        NetworkManager.Instance.SendPlayerUpdate(
            transform.position,
            transform.eulerAngles.y,
            controller.GetVelocity(),
            animX,
            animY,
            controller.IsGrounded(),
            input.JumpPressed,
            new Vector2(camRx, camRy)
        );
    }

    private void UpdateRemoteState()
    {
        targetPos = new Vector3(playerState.x, playerState.y, playerState.z);
        targetRot = Quaternion.Euler(0, playerState.rotationY, 0);

        if (animator)
        {
            animator.SetFloat("inputX", playerState.animInputX);
            animator.SetFloat("inputY", playerState.animInputY);
            animator.SetBool("IsGrounded", playerState.isGrounded);
            animator.SetBool("IsJumping", playerState.isJumping);
        }
    }

    private void InterpolateRemotePlayer()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * lerpSpeed);
    }
}