using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public static class Utils
{
    // Buncha general utilities and extension methods

    /// <summary>
    /// Destroys all children (Edit mode)
    /// </summary>
    /// <param name="transform">Parent transform</param>
    public static void ClearAllChildrenEditMode(this Transform transform){
        // Retrieve children in array
        var tempArray = new GameObject[transform.childCount];
        for(int i = 0; i < tempArray.Length; i++){
            tempArray[i] = transform.GetChild(i).gameObject;
        }

        foreach(var child in tempArray){
            UnityEngine.Object.DestroyImmediate(child);
        }
    }

    /// <summary>
    /// Destroys all children (Play mode)
    /// </summary>
    /// <param name="transform">Parent transform</param>
    public static void ClearAllChildren(this Transform transform){
        // Retrieve children in array
        var tempArray = new GameObject[transform.childCount];
        for(int i = 0; i < tempArray.Length; i++){
            tempArray[i] = transform.GetChild(i).gameObject;
        }

        foreach(var child in tempArray){
            UnityEngine.Object.Destroy(child);
        }
    }

    /// <summary>
    /// Returns the given vector with absolute values
    /// </summary>
    /// <param name="vec">Vector3</param>
    /// <returns>Absolute Vector</returns>
    public static Vector3 VectorAbs(Vector3 vec){
        return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
    }

    /// <summary>
    /// Returns the colliders in a hitbox
    /// </summary>
    public static Collider[] Hitbox(Vector3 pos, Quaternion rotation, Vector3 size, LayerMask mask){
        Collider[] hits = Physics.OverlapBox(pos, size/2f, rotation, mask);
        return hits;
    }
    public static Collider[] Hitbox(Transform transform, Vector3 offset, Vector3 size, LayerMask mask){
        Vector3 pos = transform.position + transform.rotation * offset;
        return Hitbox(pos, transform.rotation, size, mask);
    }

    public static void PlayAnimation(this Animator animator, string name, float transition = .1f){
        //animator.CrossFade(name, transition, 0, 0f, 0f);
        animator.CrossFade(name, transition);
    }

    public static Vector3 Flat(this Vector3 v){
        return new Vector3(v.x, 0f, v.z);
    }

    public static Transform FindRecursive(this Transform transform, string name){
        if (transform.name == name)
            return transform;
        
        foreach (Transform child in transform){
            Transform found = FindRecursive(child, name);
            if (found != null) return found;
        }

        return null;
    }
    public static List<Transform> FindAll(this Transform transform, string name){
        List<Transform> found = new List<Transform>();
        FindAllRec(transform, name, found);

        return found;
    }
    private static Transform FindAllRec(this Transform transform, string name, List<Transform> found){
        if (transform.name == name)
            found.Add(transform);
        
        foreach (Transform child in transform){
            FindAllRec(child, name, found);
        }

        return null;
    }

    public static Vector3 Vector3Random(){
        float v = 1;
        return new Vector3(
            UnityEngine.Random.Range(-v, v),
            UnityEngine.Random.Range(-v, v),
            UnityEngine.Random.Range(-v, v)
        );
    }

    /// <summary>
    /// Returns the Entity attached to the transform or its parents
    /// </summary>
    /// <param name="transform">Transform to start the search</param>
    /// <param name="depth">Numbers of parents to search into</param>
    /// <returns>Entity found or null</returns>
    public static EntityBase GetEntity(this Transform transform, int depth = 2){
        // Find EntityBase
        EntityBase res = transform.GetComponent<EntityBase>();
        if (res) return res;

        // Find in parent
        if (depth > 0 && transform.parent){
            return GetEntity(transform.parent, depth-1);
        }

        return null;
    }

    public static List<Renderer> GetRenderers(Transform start){
        List<Renderer> renderers = new List<Renderer>();
        GetRenderers(start, renderers);
        return renderers;
    }
    public static List<Renderer> GetRenderers(Transform start, List<Renderer> renderers){
        Renderer ren = start.GetComponent<Renderer>();
        if (ren){
            renderers.Add(ren);
        }
        foreach (Transform child in start){
            GetRenderers(child, renderers);
        }
        return renderers;
    }
    
    public static Vector3 NormalizedOrZero(this Vector3 vector){
        if (vector == Vector3.zero)
            return Vector3.zero;
        return vector.normalized;
    }
    
    public static float SlowSlope(float x, float slowness = 5){
        // slowness: x value at which y=1
        return x*2 / (x+slowness);
    }
}
