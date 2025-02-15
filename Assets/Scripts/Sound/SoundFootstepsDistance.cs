using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundFootstepsDistance : MonoBehaviour
{
    public float stepSize = 2f;
    private EntityBase entity;
    private float height = 2;

    private float distance = 0;
    private Vector3 position;

    void Start()
    {
        entity = GetComponent<EntityBase>();
        position = transform.position;
    }
    void Update()
    {
        Vector3 oldPos = position;
        position = transform.position;

        // Is on the ground?
        if (entity.isGrounded){
            // Add moved distance
            float add = (transform.position - oldPos).magnitude;
            distance += add;

            // Moved distance is a full step?
            while (distance > stepSize){
                distance -= stepSize;
                
                // Get material
                string material = "Concrete"; // Fallback

                // Raycast down
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit)){
                    SoundCollider colliderAudio = hit.collider.transform.GetComponent<SoundCollider>();
                    if (colliderAudio){
                        material = colliderAudio.material.ToString();
                    }
                    
                    Sounds.PlayAudio("footsteps"+material, transform.position - Vector3.down*(height/2f), .5f);
                }
                
            }
        }
    }
}
