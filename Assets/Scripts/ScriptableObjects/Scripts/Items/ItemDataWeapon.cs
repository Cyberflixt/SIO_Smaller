using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataWeapon : ItemData
{
    public float damageFactor = 1;
    public float speedFactor = 1;
    public float dashFactor = 1;
    public Transform vfxParry;
    
    public WeaponType type {
        get {return GetWeaponType();}
    }

    public WeaponType GetWeaponType(){
        if (this is ItemDataWeaponRanged ranged){
            return ranged.type;
        }
        if (this is ItemDataWeaponMelee melee){
            return melee.type;
        }
        return null;
    }
}
