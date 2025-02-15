using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationIdentity : MonoBehaviour
{
    void Update()
    {
        transform.rotation = new Quaternion();
    }
}
