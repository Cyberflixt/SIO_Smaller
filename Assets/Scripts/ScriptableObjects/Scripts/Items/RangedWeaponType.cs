using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunfireMode {
    Manual,
    Auto,
    Lazer,
};

[CreateAssetMenu(fileName = "RangedWeaponType", menuName = "Items/Weapons/RangedWeaponType", order = 1)]
public class RangedWeaponType : WeaponType
{
    [Header("Aiming")]
    public WeaponAttack aimAttack = null;
    public string aimAnimation = "Unnamed animation";
    public GunfireMode aimFireMode = GunfireMode.Auto;
    [ShowIf("isNotLazer")] public int aimBurstSize = 1;
    [ShowIf("isLazer")] public float aimLazerFrequency = .2f;
    public float aimFovFactor = .5f;

    [Header("Reloading")]
    public string reloadAnimation = "Unnamed animation";
    public float reloadDuration = .5f;
    public int magazineSize = 20;



    private bool isLazer{
        get {return aimFireMode == GunfireMode.Lazer;}
    }
    protected bool isNotLazer{
        get {return !isLazer;}
    }
}
