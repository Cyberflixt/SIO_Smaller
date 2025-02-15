using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScrollReact : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 0.1f;

    void Update()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0)
        {
            float newPosition = scrollRect.verticalNormalizedPosition + scrollDelta * scrollSpeed;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newPosition);
        }
    }
}
