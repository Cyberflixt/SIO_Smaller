using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeControl
{
    private static float timeFreezeTick = 0;
    private static float timeFreezeSpeed = 0;
    private static float timeFreezeDuration = 0;

    public static void Update(){
        // Every frame refresh timescale
        float t = (Time.unscaledTime - timeFreezeTick)/timeFreezeDuration;
        if (t > 1){
            Time.timeScale = 1;
        } else {
            // "Fade in" timescale
            t = t*t*t*t;
            Time.timeScale = Mathf.Lerp(timeFreezeSpeed, 1f, t);
        }
    }
    
    /// <summary>
    /// Make a smooth time freeze effect (x -> 1: ease-out)
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    /// <param name="speed">Timescale to start from (default 0)</param>
    public static void TimeFreeze(float duration, float speed = 0){
        timeFreezeTick = Time.unscaledTime;
        timeFreezeDuration = duration;
        timeFreezeSpeed = speed;
        Time.timeScale = speed;
    }
}
