using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemDropped : Interactible
{
    // Properties
    public ItemData item;
    public int item_quantity = 1;

    private float moveDuration = 0.2f;
    private float maxDistance = 2;
    private bool interacted = false;
    private float character_height = 1f;


    // Set message to item's name
    void Start(){
        message = item.name;
    }

    // When player interacts
    public override void Interact(Transform actor)
    {
        // Didn't already interact
        if (!interacted){
            base.Interact(actor);

            interacted = true;
            isInteractive = false;

            // Start function in parallel
            StartCoroutine(MoveTowards(actor));

            Sounds.PlayAudio(sound, transform.position, .3f, .1f);
        }
    }

    private float InverseSmooth(float x){
        float fac = 1.6f;
        float v = fac*x-fac/2;
        return v*v*v + 0.5f;
    }
    private float EaseOut(float x){
        return 1 - (1-x) * (1-x) * (1-x);
    }

    private IEnumerator MoveTowards(Transform actor)
    {
        Vector3 goal = actor.position + new Vector3(0, character_height, 0);
        
        // Save start position
        Vector3 startPosition = transform.position;

        // Calculate end position
        Vector3 direction = (goal - startPosition).normalized;
        float distance = (goal - startPosition).magnitude;
        if (distance > maxDistance)
            distance = maxDistance;
        Vector3 endPosition = startPosition + direction * distance;

        // Loop until time passed
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            // Get normalized time (0: start, 1: end)
            elapsedTime += Time.deltaTime;
            float time_linear = elapsedTime / moveDuration;
            float time_smooth = time_linear * time_linear * time_linear;
            float time_smoothb = EaseOut(time_linear);

            // Lerp towards endPosition
            transform.position = Vector3.Lerp(startPosition, endPosition, time_smoothb);

            // Wait until next frame
            yield return null;
        }

        // Final sound
        Sounds.PlayAudio(item.sound+"Grab", transform.position, 1, .1f);

        //TODO Add item to inventory

        // Destroy the item
        Destroy(gameObject);
    }

}
