using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVelocity : MonoBehaviour
{
    public new AudioSource audio;
    public float velocityMin = .1f;
    public float velocityMax = 1f;
    public float volumeMin = 0f;
    public float volumeMax = 1f;
    public float pitchMin = 1f;
    public float pitchMax = 1f;
    public float speedSmooth = .5f;


    private Vector3 oldPosition = Vector3.zero;
    private float speedAnim = 0;
    void Awake(){
        oldPosition = transform.position;   
    }
    
    // Update is called once per frame
    void Update()
    {
        float speed = (transform.position - oldPosition).magnitude / Time.deltaTime;
        speed = Mathf.Clamp(speed, velocityMin, velocityMax);
        oldPosition = transform.position;

        speedAnim = Mathf.Lerp(speedAnim, speed, 1-speedSmooth);
        float t = (speedAnim - velocityMin) / (velocityMax - velocityMin);
        if (t is float.NaN)
            t = 1;

        if (volumeMin == volumeMax){
            audio.volume = volumeMin;
        } else {
            audio.volume = Sounds.mainVolume * Mathf.Lerp(volumeMin, volumeMax, t);
        }

        if (pitchMin == pitchMax){
            audio.pitch = pitchMin;
        } else {
            audio.pitch = Mathf.Lerp(pitchMin, pitchMax, t);
        }
    }
}
