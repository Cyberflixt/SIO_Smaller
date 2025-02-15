using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DestructibleUtilities
{
    private const float push_force = 4;

    private static void DamageEntity(EntityBase entity, Vector3 center, float radius, float damage){
        // Push entity
        Vector3 dif = entity.meshCenter.position - center;
        float dist_factor = 1 - dif.magnitude / radius;
        //float fadeout = 1 - (radius*radius - dif.sqrMagnitude) / (radius*radius);

        Vector3 dir_positive = new Vector3(dif.x, dif.y > 0f ? dif.y : 0f, dif.z).normalized;

        entity.TakeDamage_Local(damage * dist_factor, dir_positive);
        if (entity is EntityCharacter character){
            character.StartDashDirection(.5f, AnimationCurve.Linear(0, push_force * dist_factor, 1, 0), dir_positive);
        }
    }
    public static void ExplosionSphere(Vector3 center, float radius, float damage){
        // Empty list of objets destroyed
        List<DestructibleWall> done = new List<DestructibleWall>();
        
        // Find objects in radius
        Collider[] colliders = Physics.OverlapSphere(center, radius);
        foreach (Collider collider in colliders){
            // Get parent's parent
            Transform parent = collider.transform.parent;
            if (parent){
                parent = parent.parent;
                if (parent){
                    // Is object destructible?
                    DestructibleWall destroy = parent.GetComponent<DestructibleWall>();
                    if (destroy && !done.Contains(destroy)){ // not already done?
                        done.Add(destroy);

                        // Apply explosion
                        destroy.Explosion(center, radius * .7f);
                    }
                }
            }

            // Found entity?
            EntityBase entity = collider.transform.GetComponent<EntityBase>();
            if (entity){
                DamageEntity(entity, center, radius, damage);
            } else {
                // Look in rigidbody's gameobject
                if (collider.attachedRigidbody){
                    entity = collider.attachedRigidbody.transform.GetComponent<EntityBase>();
                    if (entity)
                    {
                        DamageEntity(entity, center, radius, damage);
                    }
                    else
                    {
                        // Push rigidbody
                        Vector3 dif = collider.transform.position - center;
                        float dist = dif.magnitude;
                        Vector3 force = dif.normalized / (dist + 5);

                        collider.attachedRigidbody.AddForce(force);
                    }
                }
            }
        }

        TimeControl.TimeFreeze(1f, .1f);
    }
}
