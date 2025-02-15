using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EntityProp : EntityBase
{
    [SerializeField] public ObjectMaterial material = ObjectMaterial.Concrete;
    public float _fire_resistance = 0;
    [ShowIf("burnable")] public string burnSound = "fuseSparks";
    public override float fire_resistance{
        get {return _fire_resistance;}
        set {_fire_resistance = value;}
    }

    private const float push_force = 300;
    private SoundColliderInstance soundCollider;

    // Private

    protected override void TakeDamage_Apply(float damage, Vector3 direction){
        //Debug.Log("Applied top entityprop");
        if (alive){
            base.TakeDamage_Apply(damage, direction);
            if (direction != Vector3.zero){
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb)
                    rb.AddForce(direction.normalized * Utils.SlowSlope(damage) * push_force);
            }
        }
    }
    private void CollisionDamage(Collision collision){
        float maxDamage = health;

        float force = collision.impulse.magnitude * .1f;
        if (collision.rigidbody)
            force *= collision.rigidbody.mass;
        if (force > .5){
            TakeDamage(Mathf.Min(force * 5, maxDamage), collision.impulse);
        }
    }
    
    protected virtual void OnCollisionEnter(Collision collision)
    {
        CollisionDamage(collision);
        soundCollider.CollisionSound(collision);
    }

    protected new void Awake(){
        base.Awake();
        soundCollider = new SoundColliderInstance(material, GetComponent<Rigidbody>());
    }
    protected void OnEnable(){
        onDeath += OnDeath;
    }
    protected void OnDisable(){
        onDeath -= OnDeath;
    }
    
    private void OnDeath(){
        DebrisBreak.Spawn(transform);
        Destroy(gameObject);
    }
}
