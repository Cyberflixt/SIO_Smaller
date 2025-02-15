using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Items/Weapons/Item melee", order = 1)]
public class ItemDataWeaponMelee : ItemDataWeapon
{
    public new MeleeWeaponType type;
}
