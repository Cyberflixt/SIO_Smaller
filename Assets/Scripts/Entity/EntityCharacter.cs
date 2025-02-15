using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityCharacter : EntityBase
{
    // public
    public int animHurtInt = 2;
    public EntityVoice voice = null;
    public Transform eyeTransform = null;
    private new Rigidbody rigidbody;
    protected float height = 0;
    protected float radius = 0;
    protected Vector3 centerOffset;

    protected CharacterController characterController = null;
    protected CharControls charControls = null;
    private RagdollToggle ragdoll;
    private EntityBase dashMainTarget = null;
    [NonSerialized] public DurationGroup invincibility_durations = new DurationGroup();
    public override bool invincible {
        get {return !invincibility_durations.empty;}
    }

    protected override Vector3 GetEyePosition(){
        return eyeTransform.position;
    }
    protected override bool Kill(){
        bool success = base.Kill();
        if (success){
            // Sound
            voice?.deathSound.Play(transform.position);

            // Ragdoll
            if (ragdoll)
                SetRagdollEnabled(true);
            
        }
        return success;
    }

    // Hurt animation
    private int lastAnimHurtInt = 0;
    private void PlayHurtAnimation(){
        // Has hurting animations?
        if (animHurtInt > 0 && animator)
        {
            // random but can't do same in a row;
            int ran = UnityEngine.Random.Range(0, animHurtInt-1);
            if (ran == lastAnimHurtInt)
            {
                ran = animHurtInt - 1;
            }
            lastAnimHurtInt = ran;

            animator.PlayAnimation("Hurt" + ran.ToString(), .05f);
        }
    }
    public void SetRagdollEnabled(bool enable){
        if (ragdoll){
            SetStunCondition("ragdoll", enable);
            if (charControls){
                Vector3 force = charControls.GetGravityForce();
                ragdoll.SetRagdollEnabled(enable, force);
            } else {
                ragdoll.SetRagdollEnabled(enable, Vector3.zero);
            }
        }
    }


    private float groundedTime = 0;
    private void UpdateGrounded(){
        // Check with a sphere on the ground

        float coyoteJump = .1f; // Short timing to jump after leaving the ground
        float extra_height = .1f;
        float half_height = height / 2f;
        float feet_radius = radius - .01f;

        if (characterController.isGrounded || Physics.CheckSphere(transform.position + centerOffset + Vector3.down * (half_height - radius + extra_height), feet_radius, layerGround)){
            groundedTime = 0;
        } else {
            groundedTime += Time.deltaTime;
        }
        
        isGrounded = groundedTime < coyoteJump;
    }

    protected override void TakeDamage_Apply(float damage, Vector3 direction){
        Debug.Log("Applied top");
        if (alive){
            base.TakeDamage_Apply(damage, direction);

            if (alive){
                // Play Hurt animation
                PlayHurtAnimation();

                // Sound
                voice?.hurtSound.Play(transform.position, Utils.SlowSlope(damage) / 2);
                if (mesh && direction != Vector3.zero){
                    mesh.forward = -direction;
                }
                
                if (damage > 5 && direction != Vector3.zero){
                    float force = damage * .15f;
                    StartDashDirection(force * .2f, AnimationCurve.Linear(0, 1.3f, 1, 0), direction);
                }
            }
        }
    }


    #region Dash


    // Dash
    private float dashMainStartTick = -100;
    private float dashMainDuration = 0;
    private AnimationCurve dashMainStrength = AnimationCurve.Constant(0,1,0);
    private float dashDirStartTick = -100;
    private float dashDirDuration = 0;
    private AnimationCurve dashDirStrength = AnimationCurve.Constant(0,1,0);
    private Vector3 dashDirDirection = Vector3.zero;


    private void ResetDash(){
        if (charControls){
            charControls.speedFactorDash = 1f;
            charControls.target = null;
        }
    }
    private void UpdateDash(){
        // Update all dashes

        // Main dash (forward vector)
        if (dashMainDuration > 0){
            float alpha = (Time.time - dashMainStartTick) / dashMainDuration;
            if (alpha > 0){
                if (alpha < 1){
                    float lerp = 1 - alpha*alpha*alpha;
                    if (charControls){
                        charControls.speedFactorDash = 1 - lerp;
                        charControls.target = dashMainTarget;
                    }

                    // Direction
                    Vector3 direction = mesh.forward;
                    if (dashMainTarget){
                        direction = (dashMainTarget.transform.position - transform.position).normalized;
                    }

                    // Move
                    Vector3 force = dashMainStrength.Evaluate(alpha) * direction  * 10;
                    Vector3 previousVel = rigidbody!=null ? rigidbody.velocity : characterController.velocity;
                    Vector3 vel = Vector3.Lerp(previousVel, force, lerp) * Time.deltaTime;
                    if (characterController != null){
                        characterController.Move(vel);
                    } else {
                        rigidbody.MovePosition(rigidbody.position + vel);
                    }
                } else {
                    dashMainDuration = 0;
                }
            }
        }

        // Directionnal dash
        if (dashDirDuration > 0){
            float alpha = (Time.time - dashDirStartTick) / dashDirDuration;
            if (alpha > 0){
                if (alpha < 1){
                    float lerp = 1 - alpha*alpha*alpha;
                    if (charControls)
                        charControls.speedFactorDash = 1 - lerp;

                    // Move
                    Vector3 force = dashDirStrength.Evaluate(alpha) * dashDirDirection  * 10;
                    Vector3 previousVel = rigidbody!=null ? rigidbody.velocity : characterController.velocity;
                    Vector3 vel = Vector3.Lerp(previousVel, force, lerp) * Time.deltaTime;
                    if (characterController != null){
                        characterController.Move(vel);
                    } else {
                        rigidbody.MovePosition(rigidbody.position + vel);
                    }
                } else {
                    dashDirDuration = 0;
                }
            }
        }
    }

    
    public void StartDashMain(float duration, AnimationCurve strength){
        dashMainStartTick = Time.time;
        dashMainDuration = duration;
        dashMainStrength = strength;
    }
    public void StartDashMain(EntityBase target, float duration, AnimationCurve strength, float time){
        dashMainTarget = target;
        dashMainStartTick = time;
        dashMainDuration = duration;
        dashMainStrength = strength;
    }

    public void StartDashDirection(float duration, AnimationCurve strength, Vector3 direction){
        dashDirStartTick = Time.time;
        dashDirDuration = duration;
        dashDirStrength = strength;
        dashDirDirection = direction;
    }
    public void StartDashDirection(float duration, AnimationCurve strength, Vector3 direction, float time){
        dashDirStartTick = time;
        dashDirDuration = duration;
        dashDirStrength = strength;
        dashDirDirection = direction;
    }

    #endregion
    #region Events

    protected void Update(){
        // Stun
        bool stunDurations_empty = stunDurations.Update();
        stunned = !stunDurations_empty || stunConditions.HasTrue();
        invincibility_durations.Update();
        
        if (height > 0){
            UpdateGrounded();
        }

        ResetDash();
        UpdateDash();
    }

    protected new void Awake(){
        base.Awake();
        stunConditions.Add("ragdoll");
        
        if (eyeTransform == null)
            eyeTransform = meshCenter;
        
        if (mesh)
            ragdoll = mesh.GetComponent<RagdollToggle>();
        
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
            rigidbody = GetComponentInChildren<Rigidbody>();
        
        // Get collider info
        CharacterController controller = GetComponent<CharacterController>();
        if (controller){
            height = controller.height;
            radius = controller.radius;
            centerOffset = controller.center;
        } else {
            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule){
                height = capsule.height;
                radius = capsule.radius;
                centerOffset = capsule.center;
            } else {
                SphereCollider colSphere = GetComponent<SphereCollider>();
                if (colSphere){
                    height = 0;
                    radius = colSphere.radius;
                    centerOffset = colSphere.center;
                }
            }
        }

        // Get components
        animator = mesh.GetComponent<Animator>();
        if (animator == null)
            animator = mesh.GetComponentInChildren<Animator>();

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = GetComponentInChildren<CharacterController>();

        charControls = GetComponent<CharControls>();
        if (charControls == null)
            charControls = GetComponentInChildren<CharControls>();
    }

    #endregion
}
