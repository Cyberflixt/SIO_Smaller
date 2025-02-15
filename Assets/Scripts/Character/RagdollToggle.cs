using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollToggle : MonoBehaviour
{
    // All u need: SetRagdollEnabled

    // Private
    [SerializeField] private Animator animator;
    [SerializeField] private Transform ragdollRoot;


    private Rigidbody[] rigidbodies;
    private CharacterJoint[] joints;
    private Collider[] colliders;
    private bool _ragdollEnabled = false;
    

    public void SetRagdollEnabled(bool enable, Vector3 velocity){
        if (enable != _ragdollEnabled){
            _ragdollEnabled = enable;
            if (enable){
                EnableRagdoll(velocity);
            } else {
                DisableRagdoll();
            }
        }
    }
    private void SetRagdollEnabled(bool enable){
        SetRagdollEnabled(enable, Vector3.zero);
    }

    private void Awake()
    {
        rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        colliders   = ragdollRoot.GetComponentsInChildren<Collider>();
        joints      = ragdollRoot.GetComponentsInChildren<CharacterJoint>();

        // Disabled by default
        DisableRagdoll();
    }

    private void EnableRagdoll(Vector3 velocity){
        // Switch to ragdoll
        animator.enabled = false;
        foreach (CharacterJoint joint in joints)
            joint.enableCollision = true;
        foreach (Collider collider in colliders)
            collider.enabled = true;
        foreach (Rigidbody rb in rigidbodies){
            rb.isKinematic = false;
            rb.velocity = velocity;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }

        // Disable main colliders
        Transform mainCollider = transform.parent;
        if (mainCollider.GetComponent<CapsuleCollider>())
            mainCollider.GetComponent<CapsuleCollider>().enabled = false;
        if (mainCollider.GetComponent<BoxCollider>())
            mainCollider.GetComponent<BoxCollider>().enabled = false;
        if (mainCollider.GetComponent<CharacterController>())
            mainCollider.GetComponent<CharacterController>().enabled = false;
    }
    
    private void DisableRagdoll()
    {
        // Switch to animator
        animator.enabled = true;
        foreach (CharacterJoint joint in joints)
            joint.enableCollision = false;
        foreach (Collider collider in colliders)
            collider.enabled = false;
        foreach (Rigidbody rb in rigidbodies){
            rb.velocity = Vector3.zero;
            rb.detectCollisions = false;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Enable main colliders
        Transform mainCollider = transform.parent;
        if (mainCollider.GetComponent<CapsuleCollider>())
            mainCollider.GetComponent<CapsuleCollider>().enabled = true;
        if (mainCollider.GetComponent<BoxCollider>())
            mainCollider.GetComponent<BoxCollider>().enabled = true;
        if (mainCollider.GetComponent<CharacterController>())
            mainCollider.GetComponent<CharacterController>().enabled = true;
    }
}
