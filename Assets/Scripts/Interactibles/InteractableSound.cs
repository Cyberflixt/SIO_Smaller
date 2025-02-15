
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class InteractableSound : Interactible
{
    public string _sound = "Metal";
    public override string sound{
        get {return _sound;}
    }
    protected void PlayInteractSound(){
        // Actor: player/enemy who interacted
        if (sound != "")
            Sounds.PlayAudio(sound, transform.position, .3f, .1f);
    }
}
