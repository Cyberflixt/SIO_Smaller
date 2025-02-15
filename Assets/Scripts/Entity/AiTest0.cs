using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTest0 : EntityController
{
    private const float spottingRange = 10f;
    private const float moveSpeed = 5;

    [NonSerialized] private EntityAttacks charAttacks;
    [NonSerialized] private EntityBase enemySpotted;
    [NonSerialized] private Vector3 directionSmooth = Vector3.right;
    private const float attackCooldownDuration = .7f;
    private float attackCooldownEnd = 0;
    private float alertedTime = 0;

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
        alertedTime -= Time.deltaTime;
        entity.alerted = alertedTime > 0;

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
                // Blocked line of sight
                enemySpotted = null;
            } else {
                // Clear line of sight
                alertedTime = 3;

                Vector3 directionFlat = new Vector3(direction.x, 0, direction.z).normalized;
                directionSmooth = Vector3.Slerp(directionSmooth, directionFlat, Time.deltaTime * 5);
                transform.forward = directionSmooth;

                if (charAttacks.item is ItemDataWeaponMelee melee){
                    bool lightAttack = true;
                    AttackTree attackTree = charAttacks.GetNextComboAttack(melee, lightAttack);
                    
                    // In attack range?
                    if (attackTree != null && distance < attackTree.attack.range ){
                        if (Time.time > attackCooldownEnd){
                            // In range: try to attack
                            charAttacks.TryAttack();
                            attackCooldownEnd = Time.time + attackCooldownDuration;
                        }
                    } else {
                        // Out of range: move towards target
                        Move(directionFlat * moveSpeed);
                    }
                }
            }
        } else {
            // Find new enemy
            if (alertedTime > 0){
                enemySpotted = entity.GetClosestEnemy(spottingRange);
            } else {
                enemySpotted = GetClosestEnemyForward(spottingRange);
            }
        }
    }

    public virtual EntityBase GetClosestEnemyForward(float range){
        // Get objects in range
        Vector3 forward = entity.mesh.forward;
        Vector3 eyePosition = entity.eyeTransform.position;
        Collider[] colliders = Physics.OverlapSphere(eyePosition, range, layerCharacter);

        EntityBase closestEnemy = null;
        float closestDistance = range;

        foreach (Collider collider in colliders){
            // Get distance and entity
            float distance = (eyePosition - collider.transform.position).magnitude;
            EntityBase target = collider.transform.GetComponent<EntityBase>();

            // Has an entity and is closest?
            if (target && target.alive && distance < closestDistance && target.isEnemy(entity)){
                // In front?
                float dot = Vector3.Dot((target.mesh.position - eyePosition).normalized, forward);
                
                if (dot > .1f){
                    // Line of sight free?
                    if (entity.InLineOfSight(target)){
                        closestDistance = distance;
                        closestEnemy = target;
                    }
                }
            }
        }

        return closestEnemy;
    }
}
