using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeAwake : Initializable
{
    [SerializeReference] private float strength = 1f;
    [SerializeReference] private float duration = .2f;
    
    public override void Initialize()
    {
        ScreenShake.ShakeStart(transform.position, strength, duration);
    }
}
