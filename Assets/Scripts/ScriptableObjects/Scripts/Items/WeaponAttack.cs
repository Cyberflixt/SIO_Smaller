using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single attack information
/// </summary>
[Serializable]
public class WeaponAttack
{
    public string animation = "";
    public float damage = 1f;
    public float duration = .5f;
    public float range = 2;
    
    public AnimationCurve dashForce = AnimationCurve.Linear(0, 1, 1, 0);
    public float dashDuration = .5f;
    public float dashDelay = .1f;
    public bool hitboxReal = true;
    [ShowIf("hitboxReal")] public float hitboxDelay = .2f;
    [ShowIf("hitboxReal")] public Vector3 hitboxOffset = Vector3.forward;
    [ShowIf("hitboxReal")] public Vector3 hitboxSize = Vector3.one;
    
    public float screenShakeForce = 3f;
    public float screenShakeDuration = .2f;
    public bool fireDamage = false;
    public AttackVfx vfx = null;
}
