using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MeleeWeaponType", menuName = "Items/Weapons/MeleeWeaponType", order = 1)]
public class MeleeWeaponType : WeaponType
{
    [Header("Combo")]
    public float comboBreakAfter = .2f;
    public float comboBreakPenalty = 0f;
    public AttackTree comboLight = null;
    public AttackTree comboHeavy = null;
}
