using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EntityController : NetworkBehaviour
{
    /// <summary>
    /// Useful functions:
    /// TryJump()
    /// Move(Vector3 force) // will be * by deltaTime
    /// </summary>
    
    [NonSerialized] public bool isGrounded = false;
    
    [NonSerialized] private const float airDrag = 1f;
    [NonSerialized] private const float gravity = 20;
    [NonSerialized] private const float jumpingForce = 8; // 8 normal
    [NonSerialized] private const float jumpingCool = .2f;

    [NonSerialized] protected LayerMask layerCharacter;
    [NonSerialized] protected EntityCharacter entity;
    [NonSerialized] protected EntityInventory inventory;

    [NonSerialized] protected float velocity_y = 0;
    [NonSerialized] private float jumpingTick;

    [NonSerialized] private float airTimeHeight = 0;
    [NonSerialized] private CharacterController characterController;


    // Start is called before the first frame update
    protected void Awake()
    {
        // References
        entity = GetComponent<EntityCharacter>();
        characterController = GetComponent<CharacterController>();
        inventory = GetComponent<EntityInventory>();
        layerCharacter = LayerMask.GetMask("Character");
    }
    
    private void TakeFallDamage(float height){
        if (height > 8)
        {
            // Fall injury
            float damage = height * 3;
            entity.TakeDamage(damage, Vector3.zero);

            entity.animator.PlayAnimation("Fall_Damage", .1f);
            entity.AddStunDuration(1f);
        }
        else if (height > 2.5f)
        {
            // Roll
            StartRoll();
        }
    }

    private void UpdateAirTime(){
        if (isGrounded){
            float air_height = airTimeHeight - transform.position.y;
            airTimeHeight = transform.position.y;

            TakeFallDamage(air_height);
        } else {
            if (transform.position.y > airTimeHeight){
                airTimeHeight = transform.position.y;
            }
        }
    }

    protected void Update(){
        // Update vars
        isGrounded = IsGrounded();
        

        // Apply gravity
        UpdateGravityForce();
        if (characterController.enabled)
            characterController.Move(GetGravityForce() * Time.deltaTime);
        UpdateAirTime();
    }

    public Vector3 GetGravityForce(){
        return new Vector3(0, velocity_y, 0);
    }

    private Vector3 last_move = Vector3.zero;
    public void Move(Vector3 move){
        if (characterController.enabled && !entity.stunned){
            last_move = move;
            characterController.Move(move * Time.deltaTime);
        }
    }

    protected virtual void UpdateGravityForce(){
        if (isGrounded){
            // Grounded
            velocity_y = -5;
        } else {
            // Freefall
            velocity_y -= gravity * Time.deltaTime;
        }

        // Damping
        velocity_y /= 1 + Time.deltaTime * airDrag;
    }

    protected virtual bool IsGrounded(){
        return (Time.time-jumpingTick > jumpingCool) && entity.isGrounded;
    }

    protected virtual void StartJump(){
        jumpingTick = Time.time;
        velocity_y = jumpingForce;

        if (entity.animator)
            entity.animator.PlayAnimation("Male_Jump", .05f);
        if (entity.voice)
            entity.voice.jumpSound.Play(entity.meshCenter.position);
    }

    protected virtual void TryJump(){
        // Jumping
        if (isGrounded){
            StartJump();
        }
    }

    private float roll_debounce = 0;
    private const float roll_duration = .6f;
    private const float roll_delay = 1f;
    private const float perfectDodgeDuration = .2f;
    protected void TryRolling(){
        if (isGrounded && Time.time > roll_debounce && !entity.stunned){
            StartRoll();
        }
    }
    protected void StartRoll(){
        if (entity.busy || entity.stunned)
            return;
        
        entity.perfectDodgeEnd = Time.time + perfectDodgeDuration;
        roll_debounce = Time.time + roll_delay;
        entity.StartDashDirection(roll_duration, AnimationCurve.Linear(0, 1.6f, 1, 0), last_move.normalized);
        if (entity.animator)
            entity.animator.PlayAnimation("Roll");
        if (entity.voice)
            entity.voice.jumpSound.Play(entity.meshCenter.position);
        
        float activeTime = roll_duration * .8f;
        if (entity is EntityCharacter character)
            character.invincibility_durations.Add(activeTime);
        entity.busy_durations.Add(activeTime);
    }

    private const float dodgeStun = 0.5f;
    private const float dodgeDuration = 0.5f;
    public void StartPerfectDodge(EntityBase attacker){
        // Stun enemy
        attacker?.AddStunDuration(dodgeStun);

        // Remove cooldown
        entity.busy_durations.Clear();
        roll_debounce = 0;

        // Dash & animation
        entity.StartDashDirection(dodgeDuration, AnimationCurve.Linear(0, 1.6f, 1, 0), last_move.normalized);
        if (entity.animator)
            entity.animator.Play("PerfectDodge");
        if (entity.voice)
            entity.voice.jumpSound.Play(entity.meshCenter.position);
        
        // Short invincibility
        if (entity is EntityCharacter character)
            character.invincibility_durations.Add(dodgeDuration);
        TimeControl.TimeFreeze(.5f, .05f);
    }
}
