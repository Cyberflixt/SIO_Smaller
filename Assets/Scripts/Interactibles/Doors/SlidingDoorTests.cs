
using UnityEngine;

public class SlidingDoorTests : InteractableSound
{
    public override void Interact(Transform actor)
    {
        Animator anim = transform.GetComponent<Animator>();  
        anim.SetTrigger("OpenCloseSlidingDoor");
        PlayInteractSound();
    }

}
