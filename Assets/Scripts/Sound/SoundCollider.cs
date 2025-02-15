using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// Component to play sound on rigid body collisions automatically
/// </summary>
public class SoundCollider : MonoBehaviour
{
    [SerializeField] public ObjectMaterial material = ObjectMaterial.Concrete;

    
    private SoundColliderInstance soundCollider;
    private void Awake(){
        soundCollider = new SoundColliderInstance(material, GetComponent<Rigidbody>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        soundCollider.CollisionSound(collision);
    }
}

public class SoundColliderInstance
{
    [SerializeField] public ObjectMaterial material = ObjectMaterial.Concrete;

    private float previousTick = 0;
    private Rigidbody rigidbody;
    public SoundColliderInstance(ObjectMaterial material, Rigidbody rigidbody){
        this.material = material;
        this.rigidbody = rigidbody;
        previousTick = 0;
    }
    
    
    private const float cooldown = .2f; // cooldown between audio
    private const float thresholdMin = .000005f; // minimum force
    private const float thresholdLight = .4f; // maximum force
    
    private (AudioClip, string) GetSoundName(){
        // Get sound name using material and weight
        bool small = false;
        if (rigidbody){
            small = rigidbody.mass < 1;
        }

        // Return name
        string name = "Collision" + material.ToString();
        if (small){
            // Small
            AudioClip audio = Sounds.GetAudioClip(name+"Small");
            if (audio)
                return (audio, name+"Small");
        }

        // Large
        return (Sounds.GetAudioClip(name), name);
    }
    public void CollisionSound(Collision collision){
        // Is force strong enough to trigger sound?
        float force = collision.impulse.magnitude;
        if (force > thresholdMin){
            // Cooldown over?
            if (Time.time - previousTick > cooldown){
                previousTick = Time.time;

                // Get position
                Vector3 pos = collision.contacts[0].point;

                // Get audio name
                (AudioClip clip, string audioName) = GetSoundName();
                if (force < thresholdLight){
                    // Get "Light" variation if it exists
                    AudioClip found = Sounds.GetAudioClip(audioName + "Light");
                    if (found) clip = found;
                }

                // Play audio!
                float volume = Mathf.Clamp01(force * .3f)*2;
                if (clip)
                    Sounds.PlayAudio(clip, pos, volume);
                else // Sound doesn't exist
                    throw new Exception($"Sound \"{audioName}\" or its collisions variations were not found!");
            }
        }
    }
}