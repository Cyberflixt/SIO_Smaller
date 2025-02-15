using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences instance = null;
    public Transform[] boxPrefabs;
    public Material[] boxDistortionMaterials;
    
    public Transform vfxExplosion;

    
    void Awake()
    {
        if (instance == null)
            instance = this;
    }
}
