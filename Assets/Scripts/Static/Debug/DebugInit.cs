using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInit : MonoBehaviour
{
    public bool clearEachFrame = false;
    public Material material;

    public static DebugInit instance;
    void Start(){
        // Singleton pattern
        if (instance == null){
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);
    }
    void OnDrawGizmos() {
        // Draw gizmos
        DebugPlus.UpdateDebug();

        // Clear if enabled
        if (clearEachFrame)
            DebugPlus.ClearDebug();
    }
    void OnApplicationQuit(){
        DebugPlus.ClearDebug();
    }
}
