using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebrisBreakData : MonoBehaviour
{
    public Transform vfx_neutral;
    public Transform vfx_material;
    public Dictionary<Material, Transform> vfx_spe_materials;
    [SerializeField] private SerializedDictionary<Material, Transform> _vfx_spe_materials;

    [NonSerialized] public static DebrisBreakData instance;
    void Awake(){
        instance = this;
        vfx_spe_materials = _vfx_spe_materials.Build_dictionary();
    }
}
