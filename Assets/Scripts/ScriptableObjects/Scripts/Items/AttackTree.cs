
using System;
using UnityEngine;

[Serializable]
public class AttackTree
{
    public WeaponAttack attack;

    // ToggleInstance: Display button to set to null
    [SerializeField] [SerializeReference] [ToggleInstance] public AttackTree nextLight = null;
    [SerializeField] [SerializeReference] [ToggleInstance] public AttackTree nextHeavy = null;

    public AttackTree(){
        nextLight = null;
        nextHeavy = null;
    }
}
