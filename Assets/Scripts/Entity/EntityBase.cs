using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EntityBase : NetworkBehaviour
{
    // public
    [SerializeField] private float _healthMax = 100;
    [SerializeField] public Transform mesh = null;
    [SerializeField] public Transform meshCenter = null;
    [NonSerialized] public float health = 100;
    [NonSerialized] public bool isGrounded = false;
    [NonSerialized] public EntityBase enemy_spotted = null;
    [NonSerialized] public float fire_damage_factor = 1;
    [NonSerialized] public FloatGroup visibility_factors = new FloatGroup(1);
    [NonSerialized] public DurationGroup busy_durations = new DurationGroup();
    [NonSerialized] public bool alerted = false;
    private const float attackStunFactor = 1;
    
    public virtual bool invincible {
        get {return false;}
    }
    public float visibility_factor {
        get {return visibility_factors.GetMinimum();}
    }
    public bool busy{
        get {return !busy_durations.empty;}
    }
    public float healthMax {
        get {return _healthMax;}
        set {_healthMax = value;}
    }
    public virtual float fire_resistance{
        get {return 0;}
        set {}
    }
    public bool burnable {
        get {return fire_resistance > 0;}
    }
    public virtual float fire_radius{
        get {return 2;}
        set {}
    }

    
    // Read-only
    public bool alive {
        get {return health > 0;}
    }
    private bool isTeamA{
        get {return transform.tag == "Ally";}
    }
    private bool isTeamB{
        get {return transform.tag == "Enemy";}
    }
    /// <summary>
    /// Are entities in opposite teams?
    /// </summary>
    /// <param name="other">Entity to compare with</param>
    public bool isEnemy(EntityBase other){
        if (other.isTeamA)
            return isTeamB;
        if (other.isTeamB)
            return isTeamA;
        return false;
    }

    /// <summary>
    /// Are entities in the same team?
    /// </summary>
    /// <param name="other">Entity to compare with</param>
    public bool isAlly(EntityBase other){
        if (other.isTeamA)
            return isTeamA;
        if (other.isTeamB)
            return isTeamB;
        return false;
    }
    protected LayerMask layerGround;
    protected LayerMask layerCharacter;
    public DurationGroup stunDurations = new DurationGroup();
    public BoolGroup stunConditions = new BoolGroup();
    [NonSerialized] public bool stunned = false;

    // Events
    public delegate void _OnDeath();
    public _OnDeath onDeath;
    public delegate void _OnHealthChanged();
    public _OnHealthChanged onHealthChanged;

    // Init
    private bool died = false;
    [NonSerialized] public Animator animator = null;
    [NonSerialized] public float perfectDodgeEnd = 0;



    
    #region Utilities    

    protected virtual bool Kill(){
        if (!died){
            died = true;

            // Invoke event
            if (onDeath != null) onDeath.Invoke();
            return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, Vector3 direction){
        //Debug.Log("Received server Rpc "+damage);
        TakeDamageClientRpc(damage, direction);
    }
    [ClientRpc]
    public void TakeDamageClientRpc(float damage, Vector3 direction){
        //Debug.Log("Received client Rpc "+damage);
        TakeDamage_Apply(damage, direction);
    }
    public void TakeDamage(float damage, Vector3 direction, EntityBase from = null){
        // Perfect dodge?
        if (Time.time < perfectDodgeEnd && !stunned){
            EntityController controller = GetComponent<EntityController>();
            controller.StartPerfectDodge(from);
            return;
        }

        if (invincible)
            return;

        // Can parry?
        EntityAttacks attacks = GetComponent<EntityAttacks>();
        if (attacks && attacks.parrying && !stunned){
            if (from == null){
                attacks.ParryDirection(direction);
            } else {
                attacks.ParryEnemy(direction, from);
            }
            return;
        }
        
        //Debug.Log("Request "+damage);
        if (NetworkManager.Singleton == null){
            // Client only
            TakeDamage_Apply(damage, direction);
        } else {
            // Replicate multiplayer
            TakeDamageServerRpc(damage, direction);
        }
    }

    protected virtual void TakeDamage_Apply(float damage, Vector3 direction){
        //Debug.Log("Applied local "+damage);
        if (alive){
            TakeDamageRaw_Apply(damage);
        }
    }
    public virtual void TakeDamage_Local(float damage, Vector3 direction){
        if (invincible)
            return;
        TakeDamageRaw_Apply(damage);
    }

    public void TakeDamageRaw_Apply(float damage){
        if (alive){
            health -= damage;
            if (!alive){
                Kill();
            }

            if (damage > 5){
                ScreenShake.ShakeStart(transform.position, damage, Utils.SlowSlope(damage * .05f));
            }
            if (damage > 0){
                DamagePopUp.Create(transform.position + Vector3.up * 1f, Mathf.Floor(damage).ToString(), 1, Mathf.Clamp01(damage/50)*.5f + .5f);
            }
            AddStunDuration(.2f * attackStunFactor);

            if (onHealthChanged != null)
                onHealthChanged.Invoke();
        }
    }

    public void AddStunDuration(float duration){
        if (duration > 0){
            stunDurations.Add(.5f);
            EntityAttacks attacks = GetComponent<EntityAttacks>();
            if (attacks){
                attacks.StopAttackImmediate();
            }
        }
    }
    public void SetStunCondition(object key, bool value){
        stunConditions.Set(key, value);
        if (value){
            EntityAttacks attacks = GetComponent<EntityAttacks>();
            if (attacks){
                attacks.StopAttackImmediate();
            }
        }
    }
    
    public List<EntityBase> Hitbox(Vector3 offset, Vector3 size){
        // Find colliders
        Vector3 pos = meshCenter.position + mesh.TransformDirection(offset);
        Collider[] hits = Physics.OverlapBox(pos, size/2f, mesh.rotation);
        DebugPlus.DrawCube(pos, size, mesh.rotation);

        // Filter by tag
        List<EntityBase> res = new List<EntityBase>();
        foreach (Collider col in hits){
            EntityBase ent = col.transform.GetComponent<EntityBase>();
            if (ent && ent != this && !res.Contains(ent)){
                res.Add(ent);
            }
        }
        return res;
    }

    protected virtual Vector3 GetEyePosition(){
        return meshCenter.position;
    }

    /// <summary>
    /// Returns true if this Entity is in line of sight with the given Entity
    /// </summary>
    /// <param name="target">Entity to look towards</param>
    /// <returns>Line of sight is not blocked</returns>
    public virtual bool InLineOfSight(EntityBase target){
        // ~layerCharacter: raycast only non-characters (eg: a wall)
        Vector3 targetEye = target.GetEyePosition();
        Vector3 selfEye = GetEyePosition();
        
        Vector3 direction = targetEye - selfEye;
        bool blocked = Physics.Raycast(selfEye, direction, 1, ~layerCharacter);
        return !blocked;
    }


    public virtual EntityBase GetClosestEnemy(float range){
        // Get objects in range
        Vector3 eyePosition = GetEyePosition();
        Collider[] colliders = Physics.OverlapSphere(eyePosition, range, layerCharacter);

        EntityBase closestEnemy = null;
        float closestDistance = range;

        foreach (Collider collider in colliders){
            // Get distance and entity
            float distance = (eyePosition - collider.transform.position).magnitude;
            EntityBase target = collider.transform.GetComponent<EntityBase>();

            // Has an entity and is closest
            if (target && target.alive && distance < closestDistance && target.isEnemy(this)){
                // Line of sight free
                if (InLineOfSight(target)){
                    closestDistance = distance;
                    closestEnemy = target;
                }
            }
        }

        return closestEnemy;
    }

    public override string ToString()
    {
        return $"EntityBase({name}, hp:{health})";
    }

    #endregion
    #region Events

    // Methods
    protected void Awake(){
        health = healthMax;
        layerGround = LayerMask.GetMask("Default");
        layerCharacter = LayerMask.GetMask("Character");
        
        // Fallback
        if (mesh == null)
            mesh = transform;
        if (meshCenter == null)
            meshCenter = mesh;
    }

    #endregion
}
