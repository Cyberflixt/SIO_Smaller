using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ui_Rotate : MonoBehaviour
{
    // Script to always rotate a UI element
    public float speed = 2f;

    private RectTransform rect;
    void Start(){
        rect = GetComponent<RectTransform>();
    }
    void Update()
    {
        rect.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
    }
}
