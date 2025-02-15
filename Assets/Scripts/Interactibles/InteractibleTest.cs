using System.Collections;
using UnityEngine;

public class InteractibleTest : Interactible
{
    // This class will be interacted with every 3s

    private IEnumerator TestingInteract(){
        // Every 3 seconds, interact
        yield return new WaitForSeconds(3);
        Interact(null);
        StartCoroutine(TestingInteract());
    }

    // Game start
    void Awake()
    {
        // Start in parallel
        StartCoroutine(TestingInteract());
    }
    
    // Override this method
    public override void Interact(Transform actor){
        // Actor: player/thing position that interacted
        Debug.Log("Interacted with " + name);
    }
}
