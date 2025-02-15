using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Group of booleans
/// </summary>
public class BoolGroup
{
    private Dictionary<object, bool> group = new Dictionary<object, bool>();
    public void Add(object key, bool value = false)
    {
        group[key] = value;
    }
    public void Set(object key, bool value){
        group[key] = value;
    }

    public bool HasTrue(){
        foreach (bool b in group.Values){
            if (b) return true;
        }
        return false;
    }

    public bool HasFalse(){
        foreach (bool b in group.Values){
            if (!b) return true;
        }
        return false;
    }

    public bool Get(){
        return HasTrue();
    }

    public static implicit operator bool(BoolGroup group)
    {
        return group.Get();
    }
}
