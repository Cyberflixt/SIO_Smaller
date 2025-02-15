using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_detection : EntityController
{
    private const float spottingRange = 10f;
    private const float moveSpeed = 5;
    public float  view_angle = 0.866f;
    [NonSerialized] private EntityAttacks charAttacks;
    [NonSerialized] private EntityBase enemySpotted;
    [NonSerialized] private Vector3 directionSmooth = Vector3.right;
//What should the A.I do ?

//What makes the Enemy spotted :
//  Enemy is in a 60° radius in front of char
//  The collegues spotted the Enemy
// |\/|

//When Enemy is found :
//  Go towards Enemy if ? 
//  Try to go around char if ?
//  Switch to long range weap(if available) once Enemy reaches a fixed distance (For later)
//   Go back to orignal position once too far away
// |X|

//When Enemy is not found :
//  Walk around in a defined path
//  method Sweep(List<position> positions):
//      go from pos0 to posn each 3 - 8 seconds
//      then from posn to pos0 ecah 3 - 8 seconds
//  Proc idle animation at 5sec if idle for 8sec
// |X|        
public EntityBase GetClosestEnemyInAngle(float range){
        // Get objects in range
        Vector3 eyePosition = entity.eyeTransform.position;
        Collider[] colliders = Physics.OverlapSphere(eyePosition, range, layerCharacter);

        EntityBase closestEnemy = null;
        float closestDistance = range;
        Vector3 character_forward = entity.mesh.forward;

        foreach (Collider collider in colliders){
            // Get distance and entity
            float distance = (eyePosition - collider.transform.position).magnitude;
            EntityBase target = collider.transform.GetComponent<EntityBase>();
            float angle = Vector3.Dot(character_forward, (collider.transform.position - transform.position).normalized);

            // Has an entity and is closest
            if (target && target.alive && distance < closestDistance && angle >= view_angle && target.isEnemy(entity)){
                // Line of sight free
                if (entity.InLineOfSight(target)){
                    closestDistance = distance;
                    closestEnemy = target;
                }
            }
        }

        return closestEnemy;
    }
    void Start()
    {
        // Equip first inventory slot
        inventory.EquipIndex(0);

        // References
        charAttacks = GetComponent<EntityAttacks>();
    }


    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Stop if dead or stunned
        if (!entity.alive || entity.stunned)
            return;
        
        // Has AI spotted an alive enemy?
        if (enemySpotted && enemySpotted.alive){
            // Line vector towards target
            Vector3 dif = enemySpotted.transform.position - transform.position;
            float distance = dif.magnitude;
            Vector3 direction = dif / distance;
    
            // Raycast towards the target (to see if the view is blocked)
            bool blocked = Physics.Raycast(transform.position, direction, distance, ~layerCharacter); //~layerCharacter: raycast only non-characters (eg: a wall)
            if (blocked){
                // Blocked line of sight or outside 60° radius in front
                enemySpotted = null;
            } else {
                // Clear line of sight
                Vector3 directionFlat = new Vector3(direction.x, 0, direction.z).normalized;
                directionSmooth = Vector3.Slerp(directionSmooth, directionFlat, Time.deltaTime * 5);
                transform.forward = directionSmooth;

                if (charAttacks.item is ItemDataWeaponMelee melee){
                    bool lightAttack = true;
                    AttackTree attackTree = charAttacks.GetNextComboAttack(melee, lightAttack);
                    
                    // In attack range?
                    if (attackTree != null && distance < attackTree.attack.range ){
                        // In range: try to attack
                        charAttacks.TryAttack();
                    } else {
                        // Out of range: move towards target
                        Move(directionFlat * moveSpeed);
                    }
                }
            }
        } else {
            // Find new enemy
            enemySpotted = GetClosestEnemyInAngle(spottingRange);
        }
    }
}