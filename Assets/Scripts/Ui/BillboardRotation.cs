using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class BillboardRotation : MonoBehaviour
{
    void Update()
    {
        if (!Application.isPlaying && Application.isEditor){ // Edit mode
#if UNITY_EDITOR
            if (SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.camera)
                transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
#endif
        } else if (Camera.main){
            transform.forward = Camera.main.transform.forward;
            //transform.rotation = Camera.current.transform.rotation;
        }
    }
}
