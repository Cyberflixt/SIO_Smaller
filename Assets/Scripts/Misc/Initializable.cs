using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializable : MonoBehaviour
{
    public virtual void Initialize(){

    }

    public static void InitializeAll(Transform transform){
        if (!transform.gameObject.activeSelf)
            return;
        
        Initializable e = transform.GetComponent<Initializable>();
        if (e && e.enabled)
            e.Initialize();
        
        foreach (Transform child in transform){
            InitializeAll(child);
        }
    }
}
