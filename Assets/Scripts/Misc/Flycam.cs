using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flycam : MonoBehaviour
{
    public Transform prefab;

    // Private
    private float speed = 5f;
    private Transform instance;
    private Vector3 velocity = Vector3.zero;
    
    void Toggle()
    {
        if (instance){
            Destroy(instance.gameObject);
            instance = null;
        } else {
            Transform cam = Camera.main.transform;
            instance = Instantiate(prefab);
            instance.position = cam.position;
            instance.rotation = cam.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
            Toggle();
        
        if (instance){
            // Moving
            Vector3 moveWorldRaw = instance.localToWorldMatrix * InputExt.GetMoveVector();
            velocity = Vector3.Lerp(velocity, moveWorldRaw * speed, Mathf.Clamp01(Time.unscaledDeltaTime * 5));
            instance.position += velocity * Time.unscaledDeltaTime;
        }
    }
}
