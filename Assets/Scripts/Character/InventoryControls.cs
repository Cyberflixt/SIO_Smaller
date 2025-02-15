using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class InventoryControls : MonoBehaviour
{
    public static InventoryControls instance;
    private EntityInventory inventory;
    private EntityBase entity;



    private int equippedIndex = -1;
    void OnSlotN(int n){
        if (equippedIndex != n-1){
            equippedIndex = n-1;
            inventory.EquipIndex(equippedIndex);
        }
    }
    
    void Awake(){
        // OnDeath event
        inventory = GetComponent<EntityInventory>();
        entity = GetComponent<EntityBase>();
        entity.onDeath += OnDeath;
        
        // Set ref if owner
        if (entity.IsOwner || SteamManager.currentLobby == null){
            instance = this;
        }
    }

    // Disable on death
    void OnDeath(){
        enabled = false;
    }

    // Bindings
    void OnSlot1(InputAction.CallbackContext content){
        OnSlotN(1);
    }
    void OnSlot2(InputAction.CallbackContext content){
        OnSlotN(2);
    }
    void OnSlot3(InputAction.CallbackContext content){
        OnSlotN(3);
    }
    void OnSlot4(InputAction.CallbackContext content){
        OnSlotN(4);
    }
    void OnSlot5(InputAction.CallbackContext content){
        OnSlotN(5);
    }

    void OnEnable()
    {
        InputExt.actions["Slot1"].started += OnSlot1;
        InputExt.actions["Slot2"].started += OnSlot2;
        InputExt.actions["Slot3"].started += OnSlot3;
        InputExt.actions["Slot4"].started += OnSlot4;
        InputExt.actions["Slot5"].started += OnSlot5;
    }
    void OnDisable(){
        InputExt.actions["Slot1"].started -= OnSlot1;
        InputExt.actions["Slot2"].started -= OnSlot2;
        InputExt.actions["Slot3"].started -= OnSlot3;
        InputExt.actions["Slot4"].started -= OnSlot4;
        InputExt.actions["Slot5"].started -= OnSlot5;
    }
}
