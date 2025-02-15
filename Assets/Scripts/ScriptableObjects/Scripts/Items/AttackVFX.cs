using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

/*
Weapon VFX hierarchy
(all are optional)

(root prefab)
| Left: position & rotation copied from weapon's left mesh
|  | Muzzle: position & rotation copied from weapon's left muzzle
|  | Beam: its LineRenderer's position will be set between Muzzle and Impact (eg: bullet trail)
|  | Attach:       position & rotation ALWAYS copied from weapon's left bone until it gets destroyed
|  | MuzzleAttach: position & rotation ALWAYS copied from weapon's left muzzle until it gets destroyed
|
| LeftRepeat: Same as left, but spawns every time, even on burst or lazer shots
|  ~ Same as left
|
| Right: position & rotation copied from weapon's right mesh
|  | Muzzle: position & rotation copied from weapon's right muzzle
|  | Beam: its LineRenderer's position will be set between Muzzle and Impact (eg: bullet trail)
|  | Attach:       position & rotation ALWAYS copied from weapon's right bone until it gets destroyed
|  | MuzzleAttach: position & rotation ALWAYS copied from weapon's right muzzle until it gets destroyed
|
| RightRepeat: Same as left, but spawns every time, even on burst or lazer shots
|  ~ Same as left
| 
| Impact: position & normal set to surface hit (eg: gun impact on a surface)
|  | Damaged: enabled if an enemy was damaged


*/

[Serializable]
public class AttackVfx
{
    public Transform prefab;
    public float delay = 0;
    
    public async Task<AttackVfxInstance> InstantiateAsync(Transform meshLeftHand, Transform meshRightHand){
        if (prefab){
            if (delay > 0){
                await WaitForSecondsTask(delay);
            }
            return InstantiateImmediate(meshLeftHand, meshRightHand);
        }
        return null;
    }

    private AttackVfxInstance InstantiateImmediate(Transform meshLeftHand, Transform meshRightHand){
        if (prefab)
            return new AttackVfxInstance(prefab, meshLeftHand, meshRightHand);
        return null;
    }

    private async Task WaitForSecondsTask(float delay){
        float start = Time.time;
        while (Time.time - start < delay){
            await Task.Yield();
        }
    }
}



public class AttackVfxInstance
{
    public bool looping = false;

    public Vector3? beamEndPosition = null;
    private Transform instance = null;
    private Transform meshLeftHand;
    private Transform meshRightHand;
    private Transform vfxImpact = null;
    private float lifetime = 10;
    
    public AttackVfxInstance(Transform prefab, Transform meshLeftHand, Transform meshRightHand){
        this.meshLeftHand = meshLeftHand;
        this.meshRightHand = meshRightHand;
        
        // Instantiate
        instance = GameObject.Instantiate(prefab);
        if (meshRightHand){
            instance.position = meshRightHand.position;
            instance.rotation = meshRightHand.rotation;
        } else if (meshLeftHand){
            instance.position = meshLeftHand.position;
            instance.rotation = meshLeftHand.rotation;
        }

        Initialize();
        Global.instance.StartCoroutine(Cycle());
    }

    /// <summary>
    /// Initialize both left and right vfx
    /// </summary>
    private void Initialize(){
        // Disable by default
        Transform vfxImpact = instance.Find("Impact");
        if (vfxImpact) vfxImpact.gameObject.SetActive(false);
        Transform LeftRepeat = instance.Find("LeftRepeat");
        if (LeftRepeat) LeftRepeat.gameObject.SetActive(false);
        Transform RightRepeat = instance.Find("RightRepeat");
        if (RightRepeat) RightRepeat.gameObject.SetActive(false);

        // Initialize starting sides effects
        Transform vfxLeft  = instance.Find("Left");
        InitializeSide(meshLeftHand, vfxLeft);

        Transform vfxRight = instance.Find("Right");
        InitializeSide(meshRightHand, vfxRight);

        if (beamEndPosition != null){
            SetBeamEndPosition(beamEndPosition.Value);
        }
        
        SpawnRepeat();
    }
    public void SpawnRepeat(){
        Transform vfxLeftRepeat = instance.Find("LeftRepeat");
        if (vfxLeftRepeat)
            InitializeSide(meshLeftHand, GameObject.Instantiate(vfxLeftRepeat));

        Transform vfxRightRepeat = instance.Find("RightRepeat");
        if (vfxRightRepeat)
            InitializeSide(meshRightHand, GameObject.Instantiate(vfxRightRepeat));

        if (beamEndPosition != null){
            SetBeamEndPosition(beamEndPosition.Value);
        }
    }

    /// <summary>
    /// Initialize only one side vfx
    /// </summary>
    private void InitializeSide(Transform hand, Transform vfxSide){
        if (vfxSide == null) return;
        vfxSide.gameObject.SetActive(true);

        // Destroy vfx if mesh doesn't exists
        if (hand == null){
            UnityEngine.Object.Destroy(vfxSide.gameObject);
            return;
        }

        // Copy transform
        vfxSide.position = hand.position;
        vfxSide.rotation = hand.rotation;

        // Muzzle VFX
        Transform itemMuzzle = hand.Find("Muzzle");
        Transform vfxMuzzle = vfxSide.Find("Muzzle");
        if (vfxMuzzle && itemMuzzle){
            // Copy position and rotation
            vfxMuzzle.transform.position = itemMuzzle.transform.position;
            vfxMuzzle.transform.rotation = itemMuzzle.transform.rotation;
        }

        Initializable.InitializeAll(hand);
    }

    public void SetBeamEndPosition(Vector3 BeamEndPosition){
        if (beamEndPosition != null) return;
        beamEndPosition = BeamEndPosition;

        Transform vfxLeft  = instance.Find("Left");
        Transform vfxRight = instance.Find("Right");
        
        SetBeamEndPositionSide(meshLeftHand, vfxLeft, BeamEndPosition);
        SetBeamEndPositionSide(meshRightHand, vfxRight, BeamEndPosition);
    }

    private void SetBeamEndPositionSide(Transform hand, Transform vfxSide, Vector3 beamEndPosition){
        // Side exists?
        if (hand == null || vfxSide == null)
            return;

        // Beam exists?
        Transform itemMuzzle = hand.Find("Muzzle");
        Transform vfxBeam = vfxSide.Find("Beam");

        if (itemMuzzle && vfxBeam){
            LineRenderer line = vfxBeam.GetComponent<LineRenderer>();
            if (line){
                // Set beam start and end positions
                line.SetPosition(0, itemMuzzle.position);
                line.SetPosition(1, beamEndPosition);
            }
        }
    }

    public void SpawnImpact(bool damaged, Vector3 impactPosition, Vector3 impactNormal, Transform transform = null){
        if (instance && instance.Find("Impact")){
            vfxImpact = GameObject.Instantiate(instance.Find("Impact"));
            if (transform)
                vfxImpact.parent = transform;
            vfxImpact.position = impactPosition;
            vfxImpact.forward = impactNormal;
            vfxImpact.localRotation *= Quaternion.Euler(0, 0, UnityEngine.Random.Range(0,360));
            vfxImpact.gameObject.SetActive(true);

            Transform vfxImpactDamaged = vfxImpact.Find("Damaged");
            if (vfxImpactDamaged){
                vfxImpactDamaged.gameObject.SetActive(damaged);
            }

            Initializable.InitializeAll(vfxImpact);
        }
    }
    public void SpawnImpact(bool damaged, RaycastHit hit){
        SpawnImpact(damaged, hit.point, hit.normal, hit.collider.transform);
    }

    private IEnumerator Cycle(){
        Transform vfxLeft  = instance.Find("Left");
        Transform vfxRight = instance.Find("Right");
        Transform vfxLeftAttach = vfxLeft?.Find("Attach");
        Transform vfxRightAttach = vfxRight?.Find("Attach");
        Transform vfxLeftMuzzleAttach = vfxLeft?.Find("MuzzleAttach");
        Transform vfxRightMuzzleAttach = vfxRight?.Find("MuzzleAttach");
        Transform vfxLeftMuzzle  = meshLeftHand?.Find("Muzzle");
        Transform vfxRightMuzzle = meshRightHand?.Find("Muzzle");

        while (lifetime > 0 || looping){
            if (!looping)
                lifetime -= Time.deltaTime;

            // Copy transforms of attached components
            if (vfxLeftAttach && meshLeftHand){
                vfxLeftAttach.position = meshLeftHand.position;
                vfxLeftAttach.rotation = meshLeftHand.rotation;
            }
            if (vfxRightAttach && meshRightHand){
                vfxRightAttach.position = vfxRightAttach.position;
                vfxRightAttach.rotation = vfxRightAttach.rotation;
            }

            // Muzzle attach
            if (vfxLeftMuzzle && vfxLeftMuzzleAttach){
                vfxLeftMuzzleAttach.position = vfxLeftMuzzle.position;
                vfxLeftMuzzleAttach.rotation = vfxLeftMuzzle.rotation;
            }
            if (vfxRightMuzzle && vfxRightMuzzleAttach){
                vfxRightMuzzleAttach.position = vfxRightMuzzle.position;
                vfxRightMuzzleAttach.rotation = vfxRightMuzzle.rotation;
            }

            yield return null;
        }

        // Turn off particles
        if (vfxImpact)
            DisableVfxRecursive(vfxImpact);
        if (instance)
            DisableVfxRecursive(instance);

        // Wait before destroy
        yield return new WaitForSeconds(10);

        // Cleanup
        if (instance && instance.gameObject)
            GameObject.Destroy(instance.gameObject);
        if (vfxImpact)
            GameObject.Destroy(vfxImpact.gameObject);
    }

    public static implicit operator bool(AttackVfxInstance vfx){
        return vfx != null;
    }

    public void Stop(float delay = 0){
        looping = false;
        lifetime = delay;
    }
    public void StopImmediate(){
        Stop();
        if (instance){
            GameObject.Destroy(instance);
        }
    }

    private void DisableVfxRecursive(Transform transform){
        ParticleSystem ps = transform.GetComponent<ParticleSystem>();
        if (ps)
            ps.Stop();
        
        foreach(Transform child in transform){
            DisableVfxRecursive(child);
        }
    }
}
