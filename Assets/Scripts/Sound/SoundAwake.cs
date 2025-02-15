using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAwake : MonoBehaviour
{
    public new AudioSource audio;
    public string audioName = "";
    public float pitchMin = 1f;
    public float pitchMax = 1f;


    void Awake()
    {
        audio.playOnAwake = false;
        audio.Stop();

        // Volume
        audio.volume *= Sounds.mainVolume;

        // Random pitch
        if (pitchMin == pitchMax){
            if (audio.pitch != pitchMin)
                audio.pitch = pitchMin;
        } else {
            audio.pitch = Mathf.Lerp(pitchMin, pitchMax, UnityEngine.Random.Range(0f, 1f));
        }

        // Get audio clip
        if (audioName != "")
            audio.clip = Sounds.GetAudioClip(audioName);

        audio.Play();
    }
}
