using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EntityInventory : MonoBehaviour
{
    public Transform boneRight = null;
    public Transform boneLeft = null;
    public Animator animator = null;
    public List<ItemData> storage = new List<ItemData>();
    public ItemData[] hotbar = new ItemData[10];

    private Dictionary<ItemData, ItemBox> weaponBoxes = new Dictionary<ItemData, ItemBox>();

    
    
    [HideInInspector] public ItemData equippedItem = null;
    [HideInInspector] public Transform meshLeftHand = null;
    [HideInInspector] public Transform meshRightHand = null;

    private Transform[] weaponBoxPrefabs;
    private EntityAttacks charAttacks;
    private Transform characterMesh = null;
    private ItemBox boxGrabbing = null;

    private int activeHotbarInt = 0;
    private int yieldToken = 0;

    private float unequipDuration = .2f;
    private float equipDuration = .2f;
    
    public ItemData activeHotbarItem {
        get {return hotbar[activeHotbarInt];}
    }

    public void SetHotbarAt(int index, ItemData item){
        // Remove old item
        ItemData oldItem = hotbar[index];
        if (oldItem)
            DestroyItemBox(oldItem);
        
        // Add new item
        if (item){
            hotbar[index] = item;
            AddItemBox(item);
        }
    }

    void Start(){
        charAttacks = GetComponent<EntityAttacks>();
        weaponBoxPrefabs = GlobalReferences.instance.boxPrefabs;
        EntityBase entity = GetComponent<EntityBase>();
        characterMesh = entity.meshCenter;

        // Equip initialization
        for (int i=0; i<hotbar.Length; i++){
            ItemData item = hotbar[i];
            if (item)
                AddItemBox(item, null, false);
        }
    }


    #region Equip item


    public void EquipIndex(int index){
        StartCoroutine(EquipIndexAsync(index));
    }

    public IEnumerator EquipIndexAsync(int index){
        yieldToken++;
        int token = yieldToken;

        // Has item equipped?
        if (equippedItem){
            yield return UnequipAsync();
            token++;
        }
        
        if (yieldToken == token){
            // Item is not null?
            if (index >= 0 && index < hotbar.Length){
                ItemData item = hotbar[index];
                if (item){
                    // Animate box
                    animator.PlayAnimation("Equip");
                    ItemBoxGrabVfx(item);
                }
            }

            Sounds.PlayAudio("ClothRustling", transform.position);

            yield return new WaitForSeconds(equipDuration);
            if (yieldToken == token){
                EquipIndexForce(index);
            } else {
                // Equipping was cancelled
                
                // Stop box from moving towards hand
                if (boxGrabbing)
                    boxGrabbing.StartOrbiting();
                
            }
        }
    }


    public void EquipIndexForce(int index){
        activeHotbarInt = index;

        // Index in range?
        if (index >= 0 && index < hotbar.Length){
            // Equip item
            ItemData item = hotbar[index];
            equippedItem  = item;
            meshLeftHand  = null;
            meshRightHand = null;

            if (item){
                if (item.meshLeftHand){
                    meshLeftHand = Instantiate(item.meshLeftHand,  boneLeft);
                    VfxItem_Start(meshLeftHand, item);
                }
                if (item.meshRightHand){
                    meshRightHand = Instantiate(item.meshRightHand, boneRight);
                    VfxItem_Start(meshRightHand, item);
                }
                
                // Assign item to attacks
                charAttacks.SetWeaponType(item, meshLeftHand, meshRightHand);
        
                // Destroy box
                DestroyItemBox(item);

                // Holding animation
                if (item.holdingAnimation != ""){
                    animator.PlayAnimation(item.holdingAnimation, .5f);
                    animator.SetBool("Holding", true);
                }
            } else {
                charAttacks.SetWeaponType(null, null, null);
            }
        }
    }

    
    #endregion
    #region Unequip item


    public void Unequip(){
        StartCoroutine(UnequipAsync());
    }

    public IEnumerator UnequipAsync(){
        if (equippedItem){
            // Cancel token (if the token changed, the action was cancelled)
            yieldToken++;
            int token = yieldToken;

            // Animation & Sound
            animator.PlayAnimation("Unequip");
            Sounds.PlayAudio("AirWhoosh", boneRight.position, .5f);
            Sounds.PlayAudio("ClothRustling", boneRight.position);

            // Stop aiming
            if (charAttacks)
                charAttacks.AimStop();

            // VFX: Transform mesh into box
            if (meshLeftHand)
                VfxItem_End(meshLeftHand, equippedItem, unequipDuration);
            if (meshRightHand)
                VfxItem_End(meshRightHand, equippedItem, unequipDuration);
            
            yield return new WaitForSeconds(unequipDuration);

            // Not cancelled?
            if (token == yieldToken){
                UnequipForce();
            }
        }
    }

    public void UnequipForce(){
        // Unequip instantly
        if (equippedItem)
            AddItemBox(equippedItem, meshRightHand);

        if (meshLeftHand)
            Destroy(meshLeftHand.gameObject);
        if (meshRightHand)
            Destroy(meshRightHand.gameObject);

        equippedItem  = null;
        meshLeftHand  = null;
        meshRightHand = null;
        animator.SetBool("Holding", false);
    }



    #endregion
    #region VFX items

    
    private void VfxItem_Start(Transform mesh, ItemData item, float duration = .15f){
        StartCoroutine(VfxItem_Start_Async(mesh, item, duration));
    }

    private IEnumerator VfxItem_Start_Async(Transform mesh, ItemData item, float duration = .15f){
        // Animation: Box becomes item

        float noiseStrength = .01f;

        Material material_box = GlobalReferences.instance.boxDistortionMaterials[item.rarityInt];

        // Set new materials
        Transform mainTrans = mesh.Find("Main");
        Transform tempTrans = Instantiate(mainTrans, mainTrans.parent);
        tempTrans.gameObject.SetActive(true);
        mainTrans.gameObject.SetActive(false);

        // Set new materials
        List<Material> materials = new List<Material>();
        List<Renderer> tempRenderers = Utils.GetRenderers(tempTrans);
        
        foreach (Renderer renderer in tempRenderers){
            List<Material> newMats = new List<Material>();
            for (int i = 0; i < renderer.materials.Length; i++)
                newMats.Add(material_box);
            renderer.SetMaterials(newMats);
            materials.AddRange(renderer.materials);
        }

        // Animate materials properties (Turn item into a box)
        float time = 0;
        while (time < duration && mesh){
            time += Time.deltaTime;
            float alpha = time/duration;

            foreach (Material mat in materials){
                mat.SetFloat("_Strength", (1-alpha) * noiseStrength);
                mat.SetFloat("_Lerp", 1-alpha);
            }

            yield return null;
        }

        // Mesh still exists?
        if (mesh){
            // Destroy temporary object
            if (tempTrans)
                Destroy(tempTrans.gameObject);
        
            // Show normal mesh
            if (mainTrans)
                mainTrans.gameObject.SetActive(true);
        }
    }
    private void VfxItem_End(Transform mesh, ItemData item, float duration = .2f){
        StartCoroutine(VfxItem_End_Async(mesh, item, duration));
    }

    private IEnumerator VfxItem_End_Async(Transform mesh, ItemData item, float duration = .2f){
        // Animation: Box becomes item

        float noiseStrength = .01f;
        Material material_box = GlobalReferences.instance.boxDistortionMaterials[item.rarityInt];

        // Set new materials
        Transform mainTrans = mesh.Find("Main");
        Transform tempTrans = Instantiate(mainTrans, mainTrans.parent);
        tempTrans.gameObject.SetActive(true);
        mainTrans.gameObject.SetActive(false);

        // Set new materials
        List<Material> materials = new List<Material>();
        List<Renderer> tempRenderers = Utils.GetRenderers(tempTrans);
        
        foreach (Renderer renderer in tempRenderers){
            List<Material> newMats = new List<Material>();
            for (int i = 0; i < renderer.materials.Length; i++)
                newMats.Add(material_box);
            renderer.SetMaterials(newMats);
            materials.AddRange(renderer.materials);
        }

        // Animate materials properties (Turn item into a box)
        float time = 0;
        while (time < duration && mesh){
            time += Time.deltaTime;
            float alpha = time/duration;

            foreach (Material mat in materials){
                mat.SetFloat("_Strength", alpha * noiseStrength);
                mat.SetFloat("_Lerp", alpha);
            }

            yield return null;
        }
    }


    #endregion
    #region Item boxes
    

    private void ItemBoxGrabVfx(ItemData item){
        // Cancel previous box
        if (boxGrabbing)
            boxGrabbing.StartOrbiting();

        // Go towards hand
        if (weaponBoxes.ContainsKey(item)){
            ItemBox box = weaponBoxes[item];
            box.StartMovingTowards(boneRight);
            boxGrabbing = box;
        }
    }

    private void DestroyItemBox(ItemData item){
        // Destroy
        if (weaponBoxes.ContainsKey(item)){
            ItemBox box = weaponBoxes[item];
            weaponBoxes[item] = null;
            Destroy(box.gameObject);
        } else {
            throw new Exception("Item box doesn't exist for " + item);
        }
    }
    
    private void AddItemBox(ItemData item, Transform spawn = null, bool thrown = true){
        // Create box and save it

        Transform reference = weaponBoxPrefabs[item.rarityInt];

        Transform prefab = Instantiate(reference);
        ItemBox box = prefab.GetComponent<ItemBox>();
        box.owner = characterMesh;
        box.itemData = item;

        float strength = 5;
        if (spawn){
            // Spawn on hand (thrown)
            prefab.position = spawn.position;
            if (characterMesh){
                box.SetVelocity(characterMesh.transform.right * 2f * strength + characterMesh.transform.forward * strength + Utils.Vector3Random() *.05f * strength);
            } else {
                box.SetVelocity(Utils.Vector3Random() * strength);
            }
        } else {
            // No position provided
            prefab.position = transform.position;
            box.SetVelocity(Utils.Vector3Random() * strength);
        }

        weaponBoxes[item] = box;

        if (thrown){
            Sounds.PlayAudioAttach("ItemBoxThrow", prefab);
        }
    }

    #endregion
}
