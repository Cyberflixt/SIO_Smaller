using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharControls : EntityController
{
    public static CharControls instance = null;

    #region Parameters
    // Public
    public Transform characterMesh;
    public Transform cameraTarget;
    public CharCamera charCamera;
    public Animator animator;
    public Material[] interactMaterials;

    // Private instances
    private CharLegsIK legsIK;
    private SoundFootstepsDistance footsteps;
    



    // Hidden public
    [HideInInspector] public EntityBase target = null;
    public bool isCrouching {get; private set;}
    public bool isRunning {get; private set;}

    // Private settings
    private float speedWalk = 3f;
    private float speedRun = 7f;
    private float speedCrouch = 2f;
    private float moveLerpSpeed = 5f;
    private float rotLerpSpeed = 5f;
    private float stepSizeCrouch = 1.2f;
    private float stepSizeWalk = 1.5f;
    private float stepSizeRun = 2.4f;
    float pushPower = 20f; // Physics strength

    

    // Variables init
    [HideInInspector] public EntityAttacks charAttacks;
    private float speedCurrent = 3;
    private Vector3 moveSmoothWorld = Vector3.zero;
    private float animCrouchWalkRunSmooth = 0;
    
    [HideInInspector] public Vector3 forwardGoal = Vector3.forward;
    [HideInInspector] public Vector3 forwardSmooth = Vector3.forward;
    [HideInInspector] public float speedFactorDash = 1f;
    private float tiltForceSmooth = 0;
    private float tiltForce = 0;
    private Vector3 moveRawLocalOld = Vector3.zero;
    private bool crouchButtonDown = false;



    #endregion
    #region Utilities

    private Vector3 CameraToWorldSpace(Vector3 v){
        Transform cam = Camera.main.transform;
        return cam.right * v.x + cam.forward * v.z;
    }
    private void SetOpacity(Transform transform, float opacity){
        SkinnedMeshRenderer ren = transform.GetComponent<SkinnedMeshRenderer>();
        if (ren){
            foreach (Material material in ren.materials){
                material.SetFloat("_opacity", opacity);
            }
        }

        foreach (Transform child in transform){
            SetOpacity(child, opacity);
        }
    }

    #endregion

    #region Updates

    private void UpdateCharacter(){
        // Get move vector
        Vector3 moveRawLocal = InputExt.GetMoveVector();
        if (moveRawLocal != Vector3.zero && moveRawLocal != moveRawLocalOld)
            tiltForce = 1;
        moveRawLocalOld = moveRawLocal;

        // Has a target?
        Vector3 moveRawWorldFlat;
        if (target){
            // Move towards it (Eg: attacking an enemy)
            moveRawWorldFlat = (target.transform.position - transform.position).normalized;
        } else {
            // Normal movement
            moveRawWorldFlat = CameraToWorldSpace(moveRawLocal).Flat().NormalizedOrZero();
        }
        
        // Set rotation goal
        if (charAttacks.aiming){
            // Look towards aim
            Vector3 aimTarget = charAttacks.SetAimPositionScreenCenter();
            forwardGoal = (aimTarget - characterMesh.position).Flat().normalized;
        } else {
            if (moveRawWorldFlat != Vector3.zero)
                forwardGoal = moveRawWorldFlat.normalized;
        }

        // Smooth move direction
        moveSmoothWorld = Vector3.Lerp(moveSmoothWorld, moveRawWorldFlat, Mathf.Clamp01(Time.deltaTime * moveLerpSpeed));


        // Move slower backwards
        float colinearity = 1;
        if (moveSmoothWorld != Vector3.zero)
            colinearity = Vector3.Dot(moveSmoothWorld.normalized, characterMesh.forward) *.5f + .5f;
        float directionSpeed = Mathf.Lerp(.5f, 1f, colinearity);

        // Apply velocity
        float factor = speedFactorDash;
        float accelSpeed = speedCurrent * directionSpeed * factor;
        Move(moveSmoothWorld * accelSpeed);

        // Set rotation
        forwardSmooth = Vector3.Slerp(forwardSmooth, forwardGoal, Mathf.Clamp01(Time.deltaTime * rotLerpSpeed));
        characterMesh.rotation = quaternion.LookRotation(forwardSmooth, Vector3.up);

        // Tilt
        float tiltRadians = -.4f;
        tiltForce = tiltForce * (1 - Time.deltaTime * 4);
        if (moveSmoothWorld != Vector3.zero){
            Vector3 localMove = characterMesh.InverseTransformVector(moveSmoothWorld);
            Vector3 cross = Vector3.Cross(localMove, Vector3.up);

            tiltForceSmooth = Mathf.Lerp(tiltForceSmooth, tiltForce, Mathf.Clamp01(Time.deltaTime * 7));
            characterMesh.rotation *= quaternion.AxisAngle(cross, tiltForceSmooth * tiltRadians);
        }
        
        // Animator values
        animator.SetFloat("Moving", Mathf.Clamp01(moveSmoothWorld.magnitude * 1.3f - .1f));
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveZ", moveSmoothWorld.magnitude);
        
        animator.SetBool("IsGrounded", IsGrounded());
    }

    private void UpdateInputs(){

        // Walk, Run, Crouch
        float speedTarget = speedWalk;
        float stepSizeTarget = stepSizeWalk;
        float animCrouchWalkRun = 0;
        float opacity = 1.2f;

        if (InputExt.running && !isRunning){
            // Started running, set tilt force
            tiltForce = 1.5f;
        }

        // Running & Crouching
        isRunning = InputExt.running;
        if (isRunning){
            // Running
            animCrouchWalkRun = 1;
            speedTarget = speedRun;
            stepSizeTarget = stepSizeRun;
            isCrouching = false;

            SettingsDynamic.fovFactors.Set(this, 1.3f);
        } else {
            isCrouching = crouchButtonDown;
            if (isCrouching){
                // Sneaking
                speedTarget = speedCrouch;
                stepSizeTarget = stepSizeCrouch;
                animCrouchWalkRun = -1;
                opacity = .2f;

                SettingsDynamic.fovFactors.Set(this, .9f);
            } else {
                // Normal walk
                SettingsDynamic.fovFactors.Set(this, 1f);
            }
        }
        entity.visibility_factors.Set("Crouch", isCrouching ? .5f : 1);

        // View
        speedCurrent = Mathf.Lerp(speedCurrent, speedTarget, Mathf.Clamp01(Time.deltaTime * 5f));

        // Animator
        animCrouchWalkRunSmooth = Mathf.Lerp(animCrouchWalkRunSmooth, animCrouchWalkRun, Mathf.Clamp01(Time.deltaTime * 5f));
        animator.SetFloat("CrouchWalkRun", animCrouchWalkRunSmooth);

        // Footsteps
        footsteps.stepSize = Mathf.Lerp(footsteps.stepSize, stepSizeTarget, Mathf.Clamp01(Time.deltaTime * 3f));

        if (Input.GetKeyDown(KeyCode.O))
            TimeControl.TimeFreeze(1f, 0);

        // Opacity
        SetOpacity(characterMesh, opacity);
    }

    private Vector3 interactMaterialPos = Vector3.zero;
    private int interactMaterialPos_ID = Shader.PropertyToID("_InteractPos");
    private void RefreshInteractiveMaterial(){
        if (SteamManager.currentLobby == null || IsOwner){
            interactMaterialPos = Vector3.Lerp(interactMaterialPos, transform.position, Time.deltaTime * 5);
            foreach (Material mat in interactMaterials){
                mat.SetVector(interactMaterialPos_ID, interactMaterialPos);
            }
        }
    }

    private LayerMask maskHideArea;
    private void RefreshHiddenArea(){
        Vector3 pos = entity.meshCenter.position;
        bool touched = Physics.CheckSphere(pos, .1f, maskHideArea);
        
        float visiblity = touched ? 0 : 1;
        entity.visibility_factors.Add("HiddenArea", visiblity);
    }


    #endregion

    #region Events


    // Update is called once per frame
    new void Update(){
        base.Update();

        Cursor.lockState = CursorLockMode.Locked;
        bool alive = entity.alive;

        if (alive){
            UpdateInputs();
            charCamera.UpdateCameraCharacter();
        } else {
            charCamera.UpdateCameraDead();
        }
        
        if (alive && !entity.stunned){
            UpdateCharacter();
        }
        
        RefreshHiddenArea();

        // Is local player?
        if (this == instance){
            RefreshInteractiveMaterial();

            // Stealth attack
            charAttacks.RefreshStealthAttackTarget();
            List<Transform> targets = new List<Transform>();
            if (charAttacks.stealthAttackTarget){
                targets.Add(charAttacks.stealthAttackTarget.transform);
            }
            UI_StealthAttack.SetTransforms(targets);
        }
    }

    new void Awake(){
        base.Awake();

        // Layers
        maskHideArea = LayerMask.GetMask("HideArea");
        
        // References
        legsIK = characterMesh.GetComponent<CharLegsIK>();
        footsteps = GetComponent<SoundFootstepsDistance>();
        charAttacks = GetComponent<EntityAttacks>();
        entity.onDeath += OnDeath;

        entity.visibility_factors.Add("HiddenArea", 1);
        entity.visibility_factors.Add("Crouch", 1);

        SettingsDynamic.fovFactors.Add(this);

        // Is local player
        if (entity.IsOwner || SteamManager.currentLobby == null){
            instance = this;
        }
    }
    
    void OnEnable(){

        if (InputExt.IsUIActive()) return;
        
        // Inputs
        InputExt.actions["Jump"].started += OnJump;
        InputExt.actions["Fire"].started += OnFireStart;
        InputExt.actions["Fire"].canceled += OnFireEnd;
        InputExt.actions["Aim"].started  += OnAimStart;
        InputExt.actions["Aim"].canceled += OnAimEnd;
        InputExt.actions["Crouch"].started += OnCrouchStart;
        InputExt.actions["Crouch"].canceled += OnCrouchEnd;
        crouchButtonDown = InputExt.actions["Crouch"].IsPressed();
    }

    void OnDisable(){
        // Inputs
        InputExt.actions["Jump"].started -= OnJump;
        InputExt.actions["Fire"].started -= OnFireStart;
        InputExt.actions["Fire"].canceled -= OnFireEnd;
        InputExt.actions["Aim"].started  -= OnAimStart;
        InputExt.actions["Aim"].canceled -= OnAimEnd;
        InputExt.actions["Crouch"].started -= OnCrouchStart;
        InputExt.actions["Crouch"].canceled -= OnCrouchEnd;
    }

    private void OnDeath(){
        isRunning = false;
        isCrouching = false;
        charAttacks.AimStop();

        enabled = false;
    }

    void OnCrouchStart(InputAction.CallbackContext content){
        crouchButtonDown = true;
    } void OnCrouchEnd(InputAction.CallbackContext content){
        crouchButtonDown = false;
    }

    void OnAimStart(InputAction.CallbackContext content){
        if (charAttacks.item is ItemDataWeaponMelee){
            charAttacks.TryAttack(false);
        } else {
            charAttacks.AimStart();
        }
    }
    void OnAimEnd(InputAction.CallbackContext content){
        charAttacks.AimStop();
        charAttacks.StopAttack();
    }

    void OnFireStart(InputAction.CallbackContext content){
        charAttacks.TryAttack(true);
    }
    void OnFireEnd(InputAction.CallbackContext content){
        charAttacks.StopAttack();
    }

    void OnJump(InputAction.CallbackContext content){
        // Jump / Roll
        if (isRunning){
            TryJump();
        } else {
            TryRolling();
        }
    }
    
    // Push touched rigidbodies
    void OnControllerColliderHit (ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic) return;

        // Apply the push
        Vector3 force = hit.moveDirection * pushPower;
        body.AddForceAtPosition(force, hit.point);
    }

    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner){
            // Is localplayer
            
            // Spawn position
            transform.position = new Vector3(0,10,0);
        } else {
            // Not localplayer
            
            // Disable controls and camera
            enabled = false;
            charCamera.enabled = false;
            charAttacks.enabled = false;
        }
    }
    #endregion
}

