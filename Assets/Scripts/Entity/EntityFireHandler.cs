using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EntityFireData{
    public EntityBase entity;
    public Transform vfx;
    public float last_tick;

    public EntityFireData(EntityBase entity, Transform vfx){
        this.entity = entity;
        this.vfx = vfx;
        last_tick = Time.time;
    }
}

public class EntityFireHandler : MonoSingleton<EntityFireHandler>
{
    [SerializeField] private Transform fireVfx;

    // Hidden
    private const float fire_damage = 10;
    private const float cycle_min_time = .5f;
    private const float fire_spread_damage = 2;
    private static List<EntityFireData> on_fire = new List<EntityFireData>();
    
    private static void Take_fire_damage(EntityFireData data, float damage){
        // Inflict damage
        if (data.entity.fire_resistance > 0){
            // Not on fire yet
            Take_fire_resistance_damage(data.entity, damage);
        } else {
            // On fire
            data.entity.TakeDamageRaw_Apply(damage);
            if (!data.entity.alive){
                On_death(data);
            }
        }
    }
    public static void Take_fire_resistance_damage(EntityBase entity, float damage){
        // Inflict damage
        if (entity.fire_resistance > 0){
            // Not on fire yet
            entity.fire_resistance -= damage;
            if (entity.fire_resistance <= 0){
                Set_on_fire(entity);
            }
        }
    }
    private static void Set_on_fire(EntityBase entity){
        entity.fire_resistance = 0;

        // Spawn VFX
        Transform vfx;
        if (entity is EntityGrassBurn grass && grass.fireVfx){
            vfx = Instantiate(grass.fireVfx);
        } else {
            vfx = Instantiate(instance.fireVfx);
        }
        vfx.parent = entity.transform;
        vfx.localScale = Vector3.one;
        vfx.localPosition = Vector3.zero;

        // Sound
        if (entity is EntityProp prop && prop.burnSound != "")
            Sounds.PlayAudioAttach(prop.burnSound, entity.transform, 1);
        
        // Register
        on_fire.Add(new EntityFireData(entity, vfx));
    }

    private static void Stop_vfx(Transform vfx){
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps) ps.Stop();
        
        foreach (Transform child in vfx)
            Stop_vfx(child);
    }

    private static void On_death(EntityFireData data){
        // Was on fire?
        if (on_fire.Contains(data)){
            on_fire.Remove(data);
            cycle--;
        }

        // Destroy vfx
        if (data.vfx){
            Stop_vfx(data.vfx);
            Destroy(data.vfx.gameObject, 10);
        }
    }

    private static void Spread_fire(EntityBase entity, float damage){
        if (entity && entity.meshCenter){
            // Get fire radius
            float radius = entity.fire_radius + 1;
        
            // Find entities in range
            Collider[] colliders = Physics.OverlapSphere(entity.meshCenter.position, radius);
            foreach(Collider col in colliders){
                EntityBase neighbor = col.transform.GetComponent<EntityBase>();
                if (neighbor == null && col.attachedRigidbody){
                    neighbor = col.attachedRigidbody.transform.GetComponent<EntityBase>();
                }
                if (neighbor && neighbor != entity){
                    float dist = (neighbor.meshCenter.position - entity.meshCenter.position).magnitude;
                    float fac = 1 - dist / radius;
                    if (fac < .2f) fac = .2f;
                    Take_fire_resistance_damage(neighbor, damage * fac);
                }
            }
        }
    }

    private static int cycle = 0;
    private static float cycle_min_tick = 0;
    private static void Single_cycle(){
        int len = on_fire.Count;

        if (cycle < len && cycle >= 0){
            EntityFireData data = on_fire[cycle];

            // Time between previous cycle
            float time_span = Time.time - data.last_tick;
            float damage_factor = data.entity.fire_damage_factor * time_span;
            data.last_tick = Time.time;

            Take_fire_damage(data, fire_damage * damage_factor);
            Spread_fire(data.entity, fire_spread_damage * damage_factor);
        }

        // Loop cycle
        if (++cycle >= len && Time.time > cycle_min_tick){
            cycle_min_tick = Time.time + cycle_min_time;
            cycle = 0;
        }
    }
    void Update()
    {
        Single_cycle();
    }
}
