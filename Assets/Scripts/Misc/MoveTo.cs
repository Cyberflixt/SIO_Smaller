using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToConstraint : MonoBehaviour
{
    public Transform target;
    public float speed = 5;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }
}
