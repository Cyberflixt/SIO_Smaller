using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_Main : MonoBehaviour
{
    // Singleton
    public static UI_Main instance;
    private static string activeUi = "";

    public static void SetActiveUI(string name){
        if (instance == null)
            return;
        
        activeUi = name;
        foreach (Transform child in instance.transform){
            child.gameObject.SetActive(child.name == name);
        }
    }

    // Events
    private void Awake(){
        // Singleton
        if (instance){
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize all
        foreach(Transform child in transform){
            child.gameObject.SetActive(true);
        }
        // Show gameplay UI
        SetActiveUI("Gameplay");
    }

    private void Start()
    {
        InputExt.actions["Pause"].started += ExitUI;
        InputExt.actions["Return"].started += ExitUI;
    }

    private void ExitUI(InputAction.CallbackContext context)
    {
        switch (activeUi){
            case "Gameplay":
                MenuPaused.instance.PauseGame();
                break;
            case "PauseMenu":
                MenuPaused.instance.ResumeGame();
                break;
            case "Settings":
                UISettingsManager.instance.CloseSettings();
                break;
        }
    }
}
