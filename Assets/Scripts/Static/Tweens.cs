using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public static class Tweens
{
    private static Dictionary<object, Dictionary<string, uint>> tokens = new Dictionary<object, Dictionary<string, uint>>();

    /// <summary>
    /// Stop all tweens of a given instance
    /// </summary>
    /// <param name="instance"></param>
    public static void StopTween(object instance){
        if (tokens.ContainsKey(instance)){
            // increase all tokens
            Dictionary<string, uint> instanceTokens = tokens[instance];
            foreach (string key in instanceTokens.Keys){
                instanceTokens[key]++;
            }
        }
    }

    /// <summary>
    /// Stop tween of a given instance's property
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="property"></param>
    public static void StopTween(object instance, string property){
        if (tokens.ContainsKey(instance)){
            // Increase property token
            Dictionary<string, uint> instanceTokens = tokens[instance];
            if (instanceTokens.ContainsKey(property))
                instanceTokens[property]++;
            
        }
    }

    public static IEnumerator TweenRigWeight(Rig rig, float value, float duration)
    {
        // Tween rig's weight towards given value
        uint token = GetCreateToken(rig, "weight");
        float elapsedTime = 0;
        float baseValue = rig.weight;

        while (elapsedTime < duration && token == GetToken(rig, "weight"))
        {
            rig.weight = Mathf.Lerp(baseValue, value, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (token == GetToken(rig, "weight"))
            rig.weight = value;
    }

    private static uint GetCreateToken(object instance, string property){
        if (!tokens.ContainsKey(instance)){
            tokens[instance] = new Dictionary<string, uint>();
        }

        Dictionary<string, uint> instanceTokens = tokens[instance];
        if (!instanceTokens.ContainsKey(property)){
            instanceTokens[property] = 0;
        }

        return instanceTokens[property];
    }

    private static uint GetToken(object instance, string property){
        return tokens[instance][property];
    }
}
