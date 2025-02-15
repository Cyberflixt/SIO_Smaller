using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponType : ScriptableObject
{
    public new string name = "Unnamed type";
    public string animationPrefix = "ABC_";
    public WeaponAttack attackAir;

    [Header("Stealth attack")]
    public float stealthAttackRange = 1;
    public float stealthAttackDuration = 1f;
    public float stealthAttackDamage = 100;
    public float stealthAttackDamageDelay = .5f;
    public AttackVfx stealthAttackVfx;
    public string stealthAttackSelfAnimation = "Unnamed animation";
    public string stealthAttackTargetAnimation = "";
    public float stealthAttackTargetAnimationDelay = 0;
    public SoundVariant stealthAttackSound;
    public float stealthAttackSoundDelay = 0;
}
