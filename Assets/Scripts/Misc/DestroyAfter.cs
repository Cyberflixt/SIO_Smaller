using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroy after {delay} seconds
/// </summary>
public class DestroyAfter : MonoBehaviour
{
    public float delay = .5f;
    
    void Awake()
    {
        Destroy(gameObject, delay);
    }
}
