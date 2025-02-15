using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Starts static components for all levels
/// </summary>
public class Global : MonoBehaviour
{
    // Singleton
    public static Global instance;

    // References
    public Transform damagePopup;

    // Events
    void Awake(){
        // Singleton
        if (instance){
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Settings
        DebugManager.instance.enableRuntimeUI = false;

        // Initialize components
        GetComponent<InputExt>().Initialize();
        Sounds.Start();

    }
    void Update(){
        TimeControl.Update();
    }
}
