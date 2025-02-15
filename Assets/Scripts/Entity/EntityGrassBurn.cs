
using System;
using UnityEngine;


public class EntityGrassBurn : EntityBase
{
    public GameObject meshAlive = null;
    public GameObject meshDying = null;
    public GameObject meshDead = null;
    public Transform fireVfx = null;
    [SerializeField] public ObjectMaterial material = ObjectMaterial.Concrete;
    public float _fire_resistance = 1;
    [ShowIf("burnable")] public string burnSound = "fuseSparks";
    public override float fire_resistance{
        get {return _fire_resistance;}
        set {_fire_resistance = value;}
    }
    public float _fire_radius = 5;
    public override float fire_radius{
        get {return _fire_radius;}
        set {_fire_radius = value;}
    }
    [SerializeField] private MinMaxFloat random_max_hp_factor = new MinMaxFloat(.8f, 1.5f);

    private SoundColliderInstance soundCollider;
    private bool meshDyingEnabled = false;

    // Private
    
    protected virtual void OnCollisionEnter(Collision collision)
    {
        soundCollider.CollisionSound(collision);
    }
    
    protected new void Awake(){
        healthMax *= random_max_hp_factor.GetRandom();
        base.Awake();
        soundCollider = new SoundColliderInstance(material, GetComponent<Rigidbody>());
    }
    protected void OnEnable(){
        onDeath += OnDeath;
        onHealthChanged += OnHealthChanged;
    }

    protected void OnDisable(){
        onDeath -= OnDeath;
        onHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged()
    {
        if (!meshDyingEnabled && health < healthMax * .9f){
            meshDyingEnabled = true;
            if (meshDying){
                meshDying.SetActive(true);
                meshAlive.SetActive(false);
            }
        }
    }
    
    private void OnDeath(){
        if (meshAlive)
            Destroy(meshAlive);
        if (meshDying && meshDying != meshDead)
            Destroy(meshDying);
        if (meshDead)
            meshDead.SetActive(true);
        Destroy(gameObject);
    }
}
