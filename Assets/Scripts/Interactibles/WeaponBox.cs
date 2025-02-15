using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    // PUBLIC
    public ItemData itemData;
    public Transform owner;

    // PRIVATE
    private const float distance_target = .5f; // Distance from the owner
    private const float speed = 3 * 40;
    private const float torque_speed = 250;
    private const float damping = 2f; // Air friction
    private const float height_speed = .1f;
    private const float orbit_speed = .008f;
    private const float tether_speed = 1;

    private float height;
    private bool orbiting = true;
    private Vector3 velocity = Vector3.zero;
    private Transform goal;


    // Start is called before the first frame update
    void Start()
    {
        height = UnityEngine.Random.Range(-.6f, .5f);
    }

    public void SetVelocity(Vector3 velocity){
        this.velocity = velocity;
    }

    void Orbit(Vector3 center){
        // Movement
        Vector3 lookat = center - transform.position;
        
        float distance_current = lookat.magnitude;
        float strength = distance_current - distance_target;
        float distance_y = center.y - transform.position.y;

        if (strength < 0)
            strength = strength*strength*strength;
        
        Vector3 up = Vector3.up;
        Vector3 crossVector = Vector3.Cross(lookat, up);

        Vector3 force = lookat / distance_current * strength * tether_speed // Towards player
                      + distance_y * up * height_speed                      // Keep height
                      + crossVector * orbit_speed;                          // Orbit
        
        // Update velocity
        velocity += force * Time.deltaTime * speed;
        velocity /= 1 + Time.deltaTime * damping;
    }

    void MoveTowards(Vector3 point){
        Vector3 dir = point - transform.position;

        // Update velocity
        velocity += dir * Time.deltaTime * speed;
        velocity /= 1 + Time.deltaTime * damping * 5;
    }

    public void StartMovingTowards(Transform goal){
        this.goal = goal;
        orbiting = false;


        // Play VFX
        Transform fx = transform.Find("End");
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        ps.Play();

        Sounds.PlayAudioAttach("ItemBoxGrab", goal);
    }
    public void StartOrbiting(){
        orbiting = true;
    }

    void Rotate(){
        // Rotation
        float torque = Time.deltaTime * torque_speed;
        Quaternion rotation = Quaternion.Euler(velocity.x * torque, velocity.y * torque, velocity.z * torque);
        transform.rotation *= rotation;
    }


    // Update is called once per frame
    void Update()
    {
        // Movement
        if (orbiting){
            // Orbit around owner
            Vector3 center = owner.position + new Vector3(0, height, 0);
            Orbit(center);
        } else {
            // Go towards hand
            MoveTowards(goal.position);
        }
        transform.position += velocity * Time.deltaTime;

        // Rotation
        Rotate();
    }
}
