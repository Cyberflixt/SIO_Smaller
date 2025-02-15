using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBtn : MonoBehaviour
{
    [Button("Begone")]
    public void Begone()
    {
        // Begone
        Destroy(gameObject);
    }
}
