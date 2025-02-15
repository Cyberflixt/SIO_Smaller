using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteAlways]
[RequireComponent(typeof(CanvasGroup))]
public class BillboardScale : MonoBehaviour
{
    [SerializeField]
    private Vector3 size = Vector3.one;
    [SerializeField]
    private MinMaxFloat distanceLimits = new MinMaxFloat(2,20);
    private enum ActionLimits{
        Static, Fade, Disable
    }

    [SerializeField]
    private ActionLimits actionLimits  = ActionLimits.Fade;
    [SerializeField]
    private float actionTransition = 1;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    void Start(){
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get camera distance
        Vector3 cam = Camera.main.transform.position;
        if (!Application.isPlaying && Application.isEditor){ // Edit mode
#if UNITY_EDITOR
            if (SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.camera)
                cam = SceneView.lastActiveSceneView.camera.transform.position;
#endif
        }
        float dist = (cam - transform.position).magnitude;

        // Limit reached?
        switch (actionLimits){
            case ActionLimits.Fade:
                float alpha = 1;
                if (dist < distanceLimits.min){
                    alpha = 1-(distanceLimits.min - dist)/actionTransition;
                } else if (dist > distanceLimits.max){
                    alpha = 1-(dist - distanceLimits.max)/actionTransition;
                }

                canvas.enabled = alpha > 0;
                if (alpha > 0)
                    canvasGroup.alpha = alpha+.1f;
                canvasGroup.interactable = alpha > .8f;

                if (alpha>0)
                    transform.localScale = size * dist;
                break;
            case ActionLimits.Disable:
                bool enable = dist > distanceLimits.min && dist < distanceLimits.max;
                canvas.enabled = enable;

                if (enable)
                    transform.localScale = size * dist;
                break;
            case ActionLimits.Static:
                transform.localScale = size * Mathf.Clamp(dist, distanceLimits.min, distanceLimits.max);
                break;
        }
    }
}
