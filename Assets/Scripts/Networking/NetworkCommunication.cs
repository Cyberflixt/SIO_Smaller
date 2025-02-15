using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkCommunication : NetworkBehaviour
{
    CharacterController characterController;
    public Animator animator;

    void Awake(){
        characterController = GetComponent<CharacterController>();
    }
    /*

    [ServerRpc]
    public void MovePlayerServerRPC(Vector3 position)
    {
        transform.position = position;
    }

    [ServerRpc]
    public void RotatePlayerServerRPC(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    [ServerRpc]
    public void AnimatorPlayerServerRPC(bool state)
    {
        animator.SetBool("IsRunning", state);
    }
    */
}
