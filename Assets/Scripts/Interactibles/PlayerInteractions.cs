using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{ 
    Interactible ClosestInteractibleObject(Vector3 center, float radius)
    {
        Collider[] objects_in_radius = Physics.OverlapSphere(center, radius);
        float closest_distance = radius;
        Interactible closest_item = null;
        foreach (Collider objects in objects_in_radius)
        {
            Interactible interactive_item = objects.transform.GetComponent<Interactible>();
            if (interactive_item && interactive_item.isInteractive)
            {
                float distance = Vector3.Distance(transform.position, objects.transform.position);
                if (distance <= closest_distance)
                {
                    closest_distance = distance;
                    closest_item = interactive_item;
                }
            }
        }
        return closest_item;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        InputExt.actions["Interact"].started += OnInteraction;
    }

    void OnInteraction(InputAction.CallbackContext content){
        //  Interacting
        Interactible interactive_object = ClosestInteractibleObject(transform.position, 3);
        if (interactive_object != null)
            interactive_object.Interact(transform);
    }
    
    // Update is called once per frame
    void Update()
    {
        Interactible item = ClosestInteractibleObject(transform.position, 3);
        if (item == null)
            UI_Interact.Hide();
        else{
            UI_Interact.Show("F", item.message);
        }
    }
}
