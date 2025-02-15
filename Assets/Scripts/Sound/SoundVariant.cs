using UnityEngine;
using System;


[Serializable]
public class SoundVariant
{
    public string sound = "";
    public float volume = 1;
    public float randomPitch = .1f;

    public SoundVariant(string sound, float volume = 1, float randomPitch = 0){
        this.sound = sound;
        this.volume = volume;
        this.randomPitch = randomPitch;
    }

    public void Play(Vector3 position, float volumeFac = 1){
        Sounds.PlayAudio(sound, position, volume * volumeFac, randomPitch);
    }
    public void PlayFlat(float volumeFac = 1){
        Sounds.PlayAudioFlat(sound, volume * volumeFac, randomPitch);
    }
}