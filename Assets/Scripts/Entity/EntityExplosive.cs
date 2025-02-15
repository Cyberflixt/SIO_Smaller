using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityExplosive : EntityProp
{
    public float explosionRadius = 10f;
    public float explosionDamage = 200f;

    protected new void OnEnable(){
        base.OnEnable();
        onDeath += OnDeath;
    }
    protected new void OnDisable(){
        base.OnDisable();
        onDeath -= OnDeath;
    }
    
    private void OnDeath(){
        DestructibleUtilities.ExplosionSphere(transform.position, explosionRadius, explosionDamage);
        Sounds.PlayAudio("Explosion", transform.position, 3);
        
        Transform vfx = Instantiate(GlobalReferences.instance.vfxExplosion);
        vfx.position = meshCenter.position;
        Destroy(vfx.gameObject, 10);
    }

    protected override void TakeDamage_Apply(float damage, Vector3 direction){
        if (alive){
            base.TakeDamage_Apply(damage, direction);
            if (burnable){
                EntityFireHandler.Take_fire_resistance_damage(this, damage);
            }
        }
    }
}
