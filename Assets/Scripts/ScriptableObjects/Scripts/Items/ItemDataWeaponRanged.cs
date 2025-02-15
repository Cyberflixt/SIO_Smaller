using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Items/Weapons/Item ranged", order = 1)]
public class ItemDataWeaponRanged : ItemDataWeapon
{
    public new RangedWeaponType type;
    public AttackVfx vfxAim = null;
}
