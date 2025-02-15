using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiWalk : EntityController
{
    private const float spottingRange = 10f;
    private const float moveSpeed = 7;

    [NonSerialized] private EntityBase enemySpotted;
    [NonSerialized] private Vector3 directionSmooth = Vector3.right;
    [NonSerialized] private Transform enemyEye;

    void Start()
    {
        // Equip first inventory slot
        //inventory.EquipIndex(0);
    }

    private float moveSmooth = 0;

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Stop if dead or stunned
        if (!entity.alive || entity.stunned)
            return;
        
        float moveTarget = 0;

        // Has AI spotted an alive enemy?
        if (enemySpotted && enemySpotted.alive){
            // Line vector towards target
            Vector3 dif = enemyEye.position - transform.position;
            float distance = dif.magnitude;
            Vector3 direction = dif / distance;

            // Raycast towards the target (to see if the view is blocked)
            bool blocked = Physics.Raycast(transform.position, direction, distance, ~layerCharacter); //~layerCharacter: raycast only non-characters (eg: a wall)
            if (blocked){
                // Blocked line of sight
                enemySpotted = null;
            } else {
                // Clear line of sight
                Vector3 directionFlat = new Vector3(direction.x, 0, direction.z).normalized;
                directionSmooth = Vector3.Slerp(directionSmooth, directionFlat, Time.deltaTime * 5);
                transform.forward = directionSmooth;

                if (distance > 3){
                    Move(directionFlat * moveSpeed * moveSmooth);
                    moveTarget = 1;
                }
            }
        } else {
            // Find new enemy
            enemySpotted = entity.GetClosestEnemy(spottingRange);
            if (enemySpotted){
                if (enemySpotted is EntityCharacter character){
                    enemyEye = character.eyeTransform;
                } else {
                    enemyEye = enemySpotted.meshCenter;
                }
            }
        }

        moveSmooth = Mathf.Lerp(moveSmooth, moveTarget, Time.deltaTime * 5);
        entity.animator.SetFloat("Moving", moveSmooth);
    }
}
