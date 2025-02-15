using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAttacks : MonoBehaviour
{
    // PUBLIC
    public bool canParry = true;
    public CharBodyIK charBodyIK = null;
    
    public ItemData item{
        get {return _itemData;}
        private set {_itemData = value;}
    }
    public bool aiming{
        get {return _aiming;}
        private set {_aiming = value;}
    }
    public bool parrying {
        get {return Time.time < parryEnd && canParry;}
    }

    // PRIVATE
    private const float teamDamage = 0.5f; // half damage
    private const float parryTimespan = .1f;
    private const float parryStun = .5f;

    // Declarations
    private ItemData _itemData;
    private Transform meshRightHand;
    private Transform meshLeftHand;
    private bool _aiming = false;
    
    private EntityBase entity;
    private CharControls charControls;

    // Attack
    private AttackVfxInstance vfxAttackInstance = null;
    private int attackToken = 0;
    private AttackTree currentComboAttack = null;
    private float currentComboEnd = 0;
    private float currentComboPenaltyEnd = 0;
    private AttackVfxInstance vfxAimInstance = null;
    private bool aimAttacking = false;
    private float aimAttackLazerTick = 0;
    private float parryEnd = 0;
    private LayerMask layerCharacter;

    


    #region Attacks

    private async void AttackComboVfx(AttackVfx vfxRef){
        int token = attackToken;
        AttackVfxInstance created = await vfxRef.InstantiateAsync(meshLeftHand, meshRightHand);
        if (token == attackToken){
            vfxAttackInstance = created;
        } else {
            created.StopImmediate();
        }
    }
    private void StartComboAttack(AttackTree tree, EntityBase target = null){
        // Save new attack
        attackToken++;
        currentComboAttack = tree;

        // Set delays
        WeaponAttack attack = tree.attack;
        ItemDataWeapon weapon = item as ItemDataWeapon;
        ItemDataWeaponMelee melee = item as ItemDataWeaponMelee;
        currentComboEnd = Time.time + attack.duration + melee.type.comboBreakAfter;
        currentComboPenaltyEnd = currentComboEnd + melee.type.comboBreakPenalty;
        parryEnd = Time.time + parryTimespan;
        entity.busy_durations.Add(attack.duration);

        // Perform attack
        entity.animator?.PlayAnimation(weapon.type.animationPrefix + attack.animation);
        
        if (entity is EntityCharacter character){
            // Sound
            character.voice.attackSound.Play(entity.meshCenter.position);
            // Dash towards enemy
            character.StartDashMain(target, attack.dashDuration, attack.dashForce, Time.time + attack.dashDelay);
        }
        
        // Screenshake
        if (attack.screenShakeForce > 0)
            ScreenShake.ShakeStart(entity.meshCenter.position, attack.screenShakeForce, attack.screenShakeDuration, Time.time + attack.hitboxDelay);

        // Hitbox
        StartCoroutine(HitboxAfter(attack, entity.mesh.transform));
        
        // VFX
        AttackVfx vfxRef = attack.vfx;
        if (vfxRef != null){
            AttackComboVfx(vfxRef);
        }
    }

    public void ParryEnemy(Vector3 direction, EntityBase enemy){
        // Stun enemy
        enemy.AddStunDuration(parryStun);

        // Push enemy
        if (enemy != null && enemy is EntityCharacter enemy_character)
            enemy_character.StartDashDirection(.3f, AnimationCurve.Linear(0,1,1,0), direction * -2);
        
        ParryDirection(direction);
    }
    public void ParryDirection(Vector3 direction){
        StopAttackImmediate();
        ItemDataWeapon weapon = item as ItemDataWeapon;
        
        entity.busy_durations.Clear();
        entity.animator?.Play(weapon.type.animationPrefix + "Parry");
        TimeControl.TimeFreeze(.2f, 0);

        // Push us away
        if (entity is EntityCharacter character)
            character.StartDashDirection(.3f, AnimationCurve.Linear(0,1,1,0), direction * 2);
        
        // If character has controls, make changes to those
        if (charControls){
            // Set character rotation
            charControls.forwardGoal = -direction;
            charControls.forwardSmooth = -direction;
            charControls.characterMesh.forward = -direction;

            // Set FOV
            charControls.charCamera.fovCurrent *= .85f;

            // Spawn VFX
            Transform vfx = Instantiate(weapon.vfxParry);
            vfx.position = entity.meshCenter.position - direction * .5f;
            vfx.forward = -direction;
            Destroy(vfx.gameObject, 3);
        }

        UI_Combo.AddCombo("Parry", 100);
        Sounds.PlayAudio("Parry", transform.position);
    }

    public AttackTree GetNextComboAttack(ItemDataWeaponMelee melee, bool lightAttack){
        // Melee weapon

        if (currentComboAttack != null && Time.time < currentComboEnd){
            // Next attack
            if (lightAttack){
                if (currentComboAttack.nextLight != null)
                    return currentComboAttack.nextLight;
            } else {
                if (currentComboAttack.nextHeavy != null)
                    return currentComboAttack.nextHeavy;
            }
        }
        if (Time.time > currentComboPenaltyEnd){
            // Start over
            return lightAttack ? melee.type.comboLight : melee.type.comboHeavy;
        }

        // Wait for penalty
        return null;
    }

    [NonSerialized] public EntityBase stealthAttackTarget = null;
    private EntityBase GetStealthAttackTarget(float radius){
        // Get all colliders in range
        Collider[] cols = Physics.OverlapSphere(entity.meshCenter.position, radius, layerCharacter);
        foreach (Collider col in cols){
            // Get EntityBase component
            EntityBase enemy = col.GetComponent<EntityBase>();
            if (enemy == null)
                enemy = col.transform.parent.GetComponent<EntityBase>();

            // Exists, is alive, is enemy, isn't alerted?
            if (enemy && enemy != entity && enemy.alive && enemy.isEnemy(entity) && !enemy.alerted){
                return enemy;
            }
        }
        return null;
    }

    private IEnumerator PlayAnimationAfter(EntityBase target, float delay, string animation){
        yield return new WaitForSeconds(delay);
        // Still alive?
        if (target && target.alive){
            target.animator.PlayAnimation(animation);
        }
    }
    private IEnumerator DamageAfter(EntityBase target, float delay, float damage){
        yield return new WaitForSeconds(delay);

        // Still alive?
        if (target && target.alive){
            Vector3 direction = (entity.meshCenter.position - target.meshCenter.position).normalized;
            target.TakeDamage(damage, direction, entity);

            // Vfx impact
            if (vfxAttackInstance){
                Vector3 center = (entity.meshCenter.position + target.meshCenter.position) / 2;
                vfxAttackInstance.SpawnImpact(true, center, -direction, target.mesh);
            }
        }
    }
    private IEnumerator PlaySoundAfter(Transform transform, float delay, SoundVariant sound){
        yield return new WaitForSeconds(delay);
        if (transform){
            sound.Play(transform.position);
        }
    }

    private void StealthAttack(EntityBase target){
        // Perform stealth attack
        attackToken++;

        // Get weapon type
        ItemDataWeapon weapon = item as ItemDataWeapon;
        if (weapon == null) return;
        WeaponType type = weapon.type;

        // Stun both
        entity.AddStunDuration(type.stealthAttackDuration);
        target.AddStunDuration(type.stealthAttackDuration);

        // Animations
        entity.animator.PlayAnimation(type.stealthAttackSelfAnimation);
        if (type.stealthAttackTargetAnimation != "")
            StartCoroutine(PlayAnimationAfter(target, type.stealthAttackTargetAnimationDelay, type.stealthAttackTargetAnimation));

        // Damage
        StartCoroutine(DamageAfter(target, type.stealthAttackDamageDelay, type.stealthAttackDamage));

        // Fx
        AttackComboVfx(type.stealthAttackVfx);
        PlaySoundAfter(target.meshCenter, type.stealthAttackSoundDelay, type.stealthAttackSound);
    }

    public void RefreshStealthAttackTarget(){
        if (item is ItemDataWeapon weapon){
            stealthAttackTarget = GetStealthAttackTarget(weapon.type.stealthAttackRange);
        }
    }


    public void TryAttack(bool lightAttack = true){
        // TRY attacking
        if (entity.stunned || entity.busy)
            return;
        
        // Attack ended?
        if (item != null){
            // Is a weapon?
            if (item is ItemDataWeaponMelee melee){
                // Melee weapon

                if (stealthAttackTarget){
                    // Stealth attack
                    StealthAttack(stealthAttackTarget);
                } else {
                    // Normal combo attack
                    AttackTree attack = GetNextComboAttack(melee, lightAttack);
                    // Can attack?
                    if (attack != null){
                        EntityBase target = entity.GetClosestEnemy(attack.attack.range);
                        StartComboAttack(attack, target);
                    }
                }
            }
            if (item is ItemDataWeaponRanged ranged){
                // Ranged weapon

                if (stealthAttackTarget){
                    // Stealth attack
                    StealthAttack(stealthAttackTarget);
                } else {
                    // Aiming?
                    if (aiming && ranged.type.aimAttack != null){
                        AimAttack();
                        return;
                    }
                }
            }
        }
    }
    public void StopAttack(){
        // Stop aim attack (eg: flamethrower)
        if (aimAttacking){
            aimAttacking = false;
            if (vfxAimInstance){
                vfxAimInstance.Stop();
            }
        }

    }
    public void StopAttackImmediate(){
        attackToken++;
        StopAttack();
    }


    #endregion
    #region Utilities


    public void SetWeaponType(ItemData item, Transform meshLeftHand, Transform meshRightHand){
        this.item = item;
        this.meshLeftHand = meshLeftHand;
        this.meshRightHand = meshRightHand;
        AimStop();
        
        // Is a gun?
        if (item != null && item is ItemDataWeaponRanged ranged){
            CharCamera.fovAimingFactor = ranged.type.aimFovFactor;
        }
    }
    
    private List<EntityBase> Hitbox(Transform transform, Vector3 offset, Vector3 size){
        // Find colliders
        Vector3 pos = transform.TransformVector(offset) + entity.meshCenter.position;
        Collider[] hits = Physics.OverlapBox(pos, size/2f, transform.rotation);
        DebugPlus.DrawCube(pos, size, transform.rotation);

        // Filter by tag
        List<EntityBase> res = new List<EntityBase>();
        foreach (Collider col in hits){
            EntityBase ent = col.transform.GetComponent<EntityBase>();
            if (ent && ent != entity && !res.Contains(ent)){
                res.Add(ent);
            }
        }
        return res;
    }

    private IEnumerator HitboxAfter(WeaponAttack data, Transform transform){
        // Creates a damage hitbox after a delay

        int token = attackToken;
        yield return new WaitForSeconds(data.hitboxDelay);
        
        if (token == attackToken && entity.alive && !entity.stunned){
            List<EntityBase> hits = Hitbox(transform, data.hitboxOffset, data.hitboxSize);
            foreach (EntityBase target in hits){
                // Team damage reduced
                float damage = data.damage;
                if (target.transform.tag == transform.tag)
                    damage *= teamDamage;
                
                // Apply damage
                target.TakeDamage(damage, entity.mesh.forward);

                // Fire damage?
                if (data.fireDamage)
                    EntityFireHandler.Take_fire_resistance_damage(target, damage);
                
                // Impact vfx
                if (vfxAttackInstance){
                    Vector3 center = (entity.meshCenter.position + target.meshCenter.position) / 2;
                    vfxAttackInstance.SpawnImpact(true, center, -entity.mesh.forward, target.mesh);
                }
            }
        }
    }


    #endregion
    #region Aiming


    // VFX
    private async void AimAttackVfx(bool damaged, bool touched, RaycastHit hit, Vector3 endPoint){
        int token = attackToken;
        if (vfxAimInstance != null){
            vfxAimInstance.SpawnRepeat();
        } else {
            // Create new
            ItemDataWeaponRanged gun = item as ItemDataWeaponRanged;
            vfxAimInstance = await gun.vfxAim.InstantiateAsync(meshLeftHand, meshRightHand);
            if (token != attackToken){
                vfxAimInstance.StopImmediate();
                vfxAimInstance = null;
            }

            // Lazer mode?
            if (gun.type.aimFireMode == GunfireMode.Lazer){
                vfxAimInstance.looping = true;
            } else {
                vfxAimInstance.Stop(10); // Stop after 10s
            }
        }

        // Spawn vfx
        if (touched){
            vfxAimInstance.SpawnImpact(damaged, hit);
        }
        vfxAimInstance.SetBeamEndPosition(endPoint);
    }

    private void AimAttackSingle(){
        attackToken++;

        // Set cooldown
        ItemDataWeaponRanged gun = item as ItemDataWeaponRanged;
        WeaponAttack attack = gun.type.aimAttack;
        entity.busy_durations.Add(attack.duration);

        // Raycast
        Transform camera = Camera.main.transform;
        Vector3 rayEnd = camera.position + camera.forward * attack.range;
        Ray ray = new Ray(camera.position, camera.forward * attack.range);
        bool touched = Physics.Raycast(ray, out RaycastHit hit, attack.range, ~0, QueryTriggerInteraction.Ignore); // all layers & ignore triggers
        
        Vector3 endPoint = rayEnd;

        // Bullet damage
        if (attack.hitboxReal){
            // 3D hitbox
            StartCoroutine(HitboxAfter(attack, camera.transform));
            if (gun.vfxAim != null)
                AimAttackVfx(false, touched, hit, endPoint);
        } else {
            bool damaged = false;
            if (touched){
                // Raycast shot
                endPoint = hit.point;
                EntityBase target = hit.collider.transform.GetEntity();
                if (target){
                    damaged = true;
                    target.TakeDamage(attack.damage, camera.forward);
                    if (attack.fireDamage)
                        EntityFireHandler.Take_fire_resistance_damage(target, attack.damage);
                }
            }
            if (gun.vfxAim != null)
                AimAttackVfx(damaged, touched, hit, endPoint);
        }
        
        // Screen shake
        if (attack.screenShakeForce > 0)
            ScreenShake.ShakeStart(transform.position, attack.screenShakeForce, attack.screenShakeDuration);
    }

    private void AimAttack(){
        aimAttacking = true;
        aimAttackLazerTick = Time.time;
        vfxAimInstance = null;

        AimAttackSingle();
    }

    private Vector3 aimPositionGoal = Vector3.zero;
    public void SetAimPosition(Vector3 position){
        float dist_max = 10;

        // Clamp distance (Better for smoothing)
        Vector3 dir = position - transform.position;
        float dist = dir.magnitude;
        if (dist > dist_max){
            aimPositionGoal = dir.normalized * dist_max + transform.position;
        } else {
            aimPositionGoal = position;
        }
    }
    public Vector3 SetAimPositionScreenCenter(){
        // Raycast center screen
        Transform camera = Camera.main.transform;
        Ray ray = new Ray(camera.position, camera.forward * 100);
        bool touched = Physics.Raycast(ray, out RaycastHit hit);

        // Raycast hits?
        Vector3 point;
        if (touched){
            point = hit.point;
        } else {
            point = camera.position + camera.forward * 100;
        }
        SetAimPosition(point);
        return point;
    }

    public void AimStart(){
        if (item && item is ItemDataWeaponRanged weapon && weapon.type is RangedWeaponType rangedWeaponType){
            aiming = true;

            string animation = rangedWeaponType.animationPrefix + rangedWeaponType.aimAnimation;
            entity.animator.SetBool("Aiming", true);
            entity.animator.CrossFade(animation, .1f);
            rigAimWeightGoal = 1;
            
            if (charControls){
                charControls.charCamera.panSide.Set("Aim", true);
                SettingsDynamic.sensibilityFactors.Set("Aim", SettingsDynamic.sensibilityAimFactor);
            }
            
            UI_Cursor.SetVisible(true);
        }
    }

    private float rigAimWeightGoal = 0;
    private float rigAimWeightAnim = .001f;

    public void AimStop(){
        aiming = false;
        entity.animator.SetBool("Aiming", false);
        rigAimWeightGoal = 0;

        if (charControls){
            charControls.charCamera.panSide.Set("Aim", false);
            SettingsDynamic.sensibilityFactors.Set("Aim", 1);
        }

        UI_Cursor.SetVisible(false);
    }


    #endregion
    #region Events


    void Awake()
    {
        entity = GetComponent<EntityBase>();
        charControls = GetComponent<CharControls>();
        layerCharacter = LayerMask.GetMask("Character");

        if (charControls){
            charControls.charCamera.panSide.Add("Aim");
            SettingsDynamic.sensibilityFactors.Add("Aim");
        }
    }

    void Update(){
        entity.busy_durations.Update();

        // Holding aim attack?
        if (aimAttacking){
            // Is weapon automatic?
            ItemDataWeaponRanged gun = (ItemDataWeaponRanged)item;
            if (gun.type.aimFireMode is GunfireMode.Lazer or GunfireMode.Auto)
            {
                // Fire weapon if cooldown over
                while (Time.time > aimAttackLazerTick){
                    aimAttackLazerTick += gun.type.aimLazerFrequency;
                    AimAttackSingle();
                }
            }
        }
    }

    void LateUpdate(){
        float rigAimWeightLerp = 3;
        rigAimWeightAnim = Mathf.Lerp(rigAimWeightAnim, rigAimWeightGoal, Mathf.Clamp01(Time.deltaTime * rigAimWeightLerp));

        if (charBodyIK){
            charBodyIK.aiming = aiming;
        }
    }

    #endregion
}
