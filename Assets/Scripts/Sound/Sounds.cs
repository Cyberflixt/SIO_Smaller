using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    // Singleton
    public static Sounds instance;
    [SerializeField] private Transform prefab_Sound3D;
    [SerializeField] private Transform prefab_SoundFlat;
    public static float mainVolume = .5f;
    public static Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();
    private static Dictionary<string, int> audiosArrayMax = new Dictionary<string, int>();


    // Events
    void Awake(){
        // Singleton
        if (instance){
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static void Start()
    {
        // Preload
        AudioClip[] allAudios = Resources.LoadAll<AudioClip>("Audio");
        foreach(AudioClip audio in allAudios){
            audios[audio.name] = audio;
            
            // Save the max number at the end
            // 1. Remove numbers at the end
            string prefix = audio.name;
            int num = 0;
            int factor = 1;
            bool hasSuffix = false;
            while (prefix[prefix.Length-1] >= '0' && prefix[prefix.Length-1] <= '9'){
                num += (prefix[prefix.Length-1]-'0') * factor;
                prefix = prefix.Substring(0, prefix.Length-2);
                hasSuffix = true;
            }

            // Has a number at the end?
            if (hasSuffix){
                // Save max
                if (audiosArrayMax.ContainsKey(prefix)){
                    if (audiosArrayMax[prefix] < num)
                        audiosArrayMax[prefix] = num;
                } else {
                    audiosArrayMax[prefix] = num;
                }
            }
        }
    }

    private static int GetAudioPrefixMax(string audioName){
        int max = -1;
        while (audios.ContainsKey(audioName + (max+1))){
            max++;
        }
        return max;
    }

    /// <summary>
    /// Get AudioClip of name
    /// </summary>
    /// <param name="audioName">Name of the audio clip (random prefix will be added if not found)</param>
    /// <returns></returns>
    public static AudioClip GetAudioClip(string audioName){
        if (audios.ContainsKey(audioName))
            return audios[audioName];

        int max = GetAudioPrefixMax(audioName);
        if (max > -1){
            return audios[audioName + UnityEngine.Random.Range(0, max)];
        }

        return null;
    }

    public static void PlayClipOn(Transform parent, AudioClip audioClip, float volume = 1, float randomPitch = 0){
        AudioSource audioSource = parent.GetComponent<AudioSource>();

        // Set audio and volume
        audioSource.clip = audioClip;
        audioSource.volume = volume * mainVolume;
        if (randomPitch != 0)
            audioSource.pitch += UnityEngine.Random.Range(-randomPitch, randomPitch);

        // Play sound
        audioSource.Play();

        // Delete after time
        float clipLength = audioClip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    /// <summary>
    /// Will search for a audio file named audioName and play it at position and at a  volume
    /// </summary>
    /// <param name="audioName">Name of the audio file to search (Audio file must be in a "Resources" folder)</param>
    /// <param name="position">3D position of the audio</param>
    /// <param name="volume">volume, default = 1</param>
    public static void PlayAudio(string audioName, Vector3 position, float volume = 1, float randomPitch = 0){
        AudioClip audioClip = GetAudioClip(audioName);
        PlayAudio(audioClip, position, volume, randomPitch);
    }

    /// <summary>
    /// Will search for a audio file named audioName and play it at position and at a  volume
    /// </summary>
    /// <param name="audioClip">AudioClip to play</param>
    /// <param name="position">3D position of the audio</param>
    /// <param name="volume">volume, default = 1</param>
    public static void PlayAudio(AudioClip audioClip, Vector3 position, float volume = 1, float randomPitch = 0){
        if (audioClip == null)
            throw new ArgumentException($"Tried to play a null AudioClip!");

        Transform audioSource = Instantiate(instance.prefab_Sound3D, position, Quaternion.identity);
        PlayClipOn(audioSource, audioClip, volume, randomPitch);
    }
    
    /// <summary>
    /// Play AudioClip without rolloff (eg: ui sound)
    /// </summary>
    public static void PlayAudioFlat(AudioClip audioClip, float volume = 1, float randomPitch = 0){
        if (audioClip == null)
            throw new ArgumentException($"Tried to play a null AudioClip!");

        Transform audioSource = Instantiate(instance.prefab_SoundFlat, Vector3.zero, Quaternion.identity);
        PlayClipOn(audioSource, audioClip, volume, randomPitch);
    }
    /// <summary>
    /// Play sound name without rolloff (eg: ui sound) by name
    /// </summary>
    public static void PlayAudioFlat(string audioName, float volume = 1, float randomPitch = 0){
        AudioClip audioClip = GetAudioClip(audioName);
        PlayAudioFlat(audioClip, volume, randomPitch);
    }

    /// <summary>
    /// Play AudioClip, parented to a transform
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="transform"></param>
    /// <param name="volume"></param>
    public static void PlayAudioAttach(AudioClip audioClip, Transform transform, float volume = 1, float randomPitch = 0){
        Transform audioSource = Instantiate(instance.prefab_Sound3D, transform);
        PlayClipOn(audioSource, audioClip, volume, randomPitch);
    }

    /// <summary>
    /// Play audio by given name, parented to a transform
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="transform"></param>
    /// <param name="volume"></param>
    public static void PlayAudioAttach(string audioName, Transform transform, float volume = 1, float randomPitch = 0){
        AudioClip audioClip = GetAudioClip(audioName);
        PlayAudioAttach(audioClip, transform, volume, randomPitch);
    }


    /// <summary>
    /// Play multiple unique audios of a range
    /// Eg: 3, "Fire" -> Fire2 Fire3 Fire4 (random choice)
    /// </summary>
    /// <param name="max">Number of unique sounds to play</param>
    /// <param name="audioName">Sounds prefix</param>
    /// <param name="position">World position of the audio</param>
    /// <param name="volume">Volume</param>
    public static void PlayAudioRandomRange(int max, string audioName, Vector3 position, float volume = 1){
        int audiosMax = GetAudioPrefixMax(audioName);

        int baseI = UnityEngine.Random.Range(0,audiosMax);
        for (int i = 0; i < max; i++){
            PlayAudio("debrisConcrete" + (baseI+i)%(audiosMax+1), position);
        }
    }
}
