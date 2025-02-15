using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Cursor : MonoBehaviour
{
    public RectTransform cursor_parent;
    public RectTransform cursor_leftBar;
    public RectTransform cursor_rightBar;

    // Private
    private static UI_Cursor instance;
    private static CanvasGroup canvasGroup;
    private static bool visible = false;
    private static int tokenAnimation = 0;

    void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = visible ? 1 : 0;
    }

    public static void SetVisible(bool show){
        if (visible != show){
            visible = show;
            instance.StartCoroutine(VisibleAnimation(visible));
        }
    }

    private static float visibileAnimationTime = 1;
    private static IEnumerator VisibleAnimation(bool visible){
        float duration = .3f;
        float angle = 180;

        // Change token
        tokenAnimation++;
        int token = tokenAnimation;

        // Start time (where it was left at)
        visibileAnimationTime = 1 - visibileAnimationTime;

        // Not finished and not cancelled?
        while (visibileAnimationTime < 1 && tokenAnimation == token){
            // Increase time
            visibileAnimationTime += Time.deltaTime / duration;

            // Animation time
            float t = 1 - (1-visibileAnimationTime) * (1-visibileAnimationTime);
            if (!visible)
                t = 1-t;
            
            // Set properties
            canvasGroup.alpha = t;
            instance.cursor_parent.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(angle, 0, t));

            // Wait next frame
            yield return null;
        }

        if (tokenAnimation == token){
            visibileAnimationTime = 1;

            if (visible){
                canvasGroup.alpha = 1;
                instance.cursor_parent.localRotation = Quaternion.Euler(0, 0, 0);
            } else {
                canvasGroup.alpha = 0;
                instance.cursor_parent.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}
