using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ItemCategory{
    Weapon,
    Material,
    Gear,
}

[CreateAssetMenu(fileName = "Item_", menuName = "Items/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public new string name = "Unnamed item";
    [Range(0f, 1f)]
    public float rarity = .5f;
    public ItemCategory category = ItemCategory.Weapon;
    public Sprite thumbnail = null;
    public Transform meshGroup;
    public Transform meshLeftHand;
    public Transform meshRightHand;
    public int price = 100;
    public string holdingAnimation = "Unnamed animation";
    public string sound = "Metal";


    public int rarityInt{
        get {return Mathf.FloorToInt(rarity * 5);}
    }


    public override string ToString()
    {
        return $"ItemData({name}: {category} = rarity {rarity}, price {price}, holding {holdingAnimation})";
    }
}
