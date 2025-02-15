using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MonoSingleton<T> : MonoBehaviour
{
    // Singleton
    public static T instance;

    // Events
    protected void Awake(){
        // Singleton
        if (instance != null){
            Destroy(gameObject);
            return;
        }
        if (this is T inherit)
            instance = inherit;
        DontDestroyOnLoad(gameObject);
    }
}

public class NetworkSingleton<T> : NetworkBehaviour
{
    // Singleton
    public static T instance;

    // Events
    protected void Awake(){
        // Singleton
        if (instance != null){
            Destroy(gameObject);
            return;
        }
        if (this is T inherit)
            instance = inherit;
        DontDestroyOnLoad(gameObject);
    }
}
