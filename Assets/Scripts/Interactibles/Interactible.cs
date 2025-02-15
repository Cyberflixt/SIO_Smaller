using System;
using System.Collections;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    public string message;
    [NonSerialized][HideInInspector] public bool isInteractive = true;
    public virtual string sound {
        get {return "GrabWhoosh";}
    }


    // Override this method in sub-classes
    public virtual void Interact(Transform actor){
        // Actor: player/enemy who interacted
        //Debug.Log(actor.name+" interacted with " + transform.name);
        Sounds.PlayAudioFlat("UiClick", 1, .1f);
    }
}
