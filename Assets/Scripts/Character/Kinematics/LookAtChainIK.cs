using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtChainIK : MonoBehaviour
{
    [SerializeField] private int ChainLength = 2;
    [Range(0, 1)] public float Weight = 1f;
    private bool GradualWeight = false;
    [NonSerialized] public float PitchOnly = 0; // Range(0,1)
    [NonSerialized] public Transform PitchSpace = null;
    
    
    public void ResolveIK_Weight(Vector3 lookDirection){
        ResolveIK_Weight_Recursive(transform, lookDirection, ChainLength);
    }

    private void ResolveIK_Weight_Recursive(Transform bone, Vector3 lookDirection, int chain_current){
        if (chain_current <= 0)
            return;
        
        // Parents first
        ResolveIK_Weight_Recursive(bone.parent, lookDirection, chain_current - 1);

        // Facing correct direction?
        Quaternion oldRot = transform.rotation;
        Quaternion goal = Quaternion.LookRotation(lookDirection);

        if (GradualWeight)
            transform.rotation = Quaternion.Lerp(transform.rotation, goal, chain_current / ChainLength * Weight);
        else
            transform.rotation = Quaternion.Lerp(transform.rotation, goal, Weight);
        
        if (PitchOnly > 0){
            Quaternion pitch = Quaternion.FromToRotation(PitchSpace.forward, lookDirection);
            Quaternion pitched = Quaternion.Lerp(oldRot, pitch * oldRot, Weight / ChainLength);
            transform.rotation = Quaternion.Lerp(transform.rotation, pitched, PitchOnly);
        }
    }
}
