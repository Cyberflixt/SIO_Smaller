using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiIdle : EntityController
{
    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Stop if dead or stunned
        if (!entity.alive || entity.stunned)
            return;
    }
}
