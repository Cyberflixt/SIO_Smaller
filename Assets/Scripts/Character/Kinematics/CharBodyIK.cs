using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharBodyIK : MonoBehaviour
{
    [NonSerialized] public bool aiming = false;
    [SerializeField] private LookAtChainIK head_IK;
    [SerializeField] private LookAtChainIK body_IK;

    private const float weight_head = 1f;
    private const float weight_torso_normal = .4f;
    private const float weight_torso_aim = 1f;
    private const float direction_lerp_speed = 20;
    
    private Animator animator;
    private Vector3 lookDirectionHead = Vector3.one;
    private float aimingSmooth = 0;

    private Vector3 GetHeadDirectionTarget(){
        // Get head looking direction
        // Mirror direction if looking backwards
        Vector3 head_look = Camera.main.transform.forward;

        Vector3 char_forward = transform.forward;
        float colinearity = Vector3.Dot(char_forward, head_look);
        if (colinearity < 0){
            // Mirror direction
            head_look -= 2 * colinearity * char_forward;
        }
        return head_look;
    }

    private void SetLookDirection(Vector3 lookTarget){
        // Smooth direction
        lookDirectionHead = Vector3.Slerp(lookDirectionHead, lookTarget, Mathf.Clamp01(Time.deltaTime * direction_lerp_speed));
        
        // Get weight
        float v = 1 - animator.GetFloat("HeadIk");
        float weight_torso = Mathf.Lerp(weight_torso_normal, weight_torso_aim, aimingSmooth);
        
        // Set kinematics
        body_IK.Weight = weight_torso * v;
        body_IK.ResolveIK_Weight(lookDirectionHead);
        body_IK.PitchOnly = aimingSmooth;
        body_IK.PitchSpace = transform;

        head_IK.Weight = weight_head * v;
        head_IK.ResolveIK_Weight(lookDirectionHead);
    }

    private void LateUpdate(){
        float aimLerp = 5f;
        aimingSmooth = Mathf.Lerp(aimingSmooth, aiming ? 1 : 0, Mathf.Clamp01(Time.deltaTime * aimLerp));

        // Get look direction
        Vector3 look = GetHeadDirectionTarget();
        SetLookDirection(look);
    }

    private void Start(){
        animator = GetComponent<Animator>();
        if (Camera.main){
            lookDirectionHead = GetHeadDirectionTarget();
        }
    }
}
