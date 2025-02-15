using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputExt : MonoBehaviour
{
    public static InputExt instance;

    public InputActionAsset inputAsset;
    public static bool running = false;
    public static Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();

    private static InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    public MenuPaused menuPaused;

    private static InputActionMap gameplayMap;
    private static InputActionMap uiMap;
    private static bool isUIActive = false;

    // Private
    static InputAction inputMove;
    static InputAction inputRunHold;
    static InputAction inputRunStart;

    public void Initialize(){
        // Singleton
        if (instance != null){
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Initialize action dictionary
        actions = new Dictionary<string, InputAction>();
        foreach(InputActionMap map in inputAsset.actionMaps){
            foreach(InputAction action in map.actions){
                actions[action.name] = action;
            }
        }
        
        Debug.Log("InputExt initialized");

        gameplayMap = inputAsset.FindActionMap("Gameplay");
        uiMap = inputAsset.FindActionMap("UI");

        if (gameplayMap == null || uiMap == null)
        {
            Debug.LogError("ActionMap Gameplay or UI unfound !");
            return;
        }

        // Special actions
        inputMove = actions["Move"];
        inputRunHold = actions["RunHold"];
        inputRunStart = actions["RunStart"];
        
        EnableGameplay();

        if (!isUIActive)
        {
            inputRunStart.started += OnRunStart;
            inputRunHold.started += OnRunStart;
            inputRunHold.canceled += OnRunStop;
        }
    }

    void OnRunStart(InputAction.CallbackContext obj){
        running = true;
    } void OnRunStop(InputAction.CallbackContext obj){
        running = false;
    }

    void Update(){
        // Running
        Vector2 move = inputMove.ReadValue<Vector2>();
        if (move == Vector2.zero){
            running = false;
        } else {
            if (running == false){
                running = actions["RunHold"].IsPressed();
            }
        }
    }

    public static Vector3 GetMoveVector(){
        Vector2 raw = inputMove.ReadValue<Vector2>();
        Vector3 move = new Vector3(raw.x, 0, raw.y);

        if (raw.x != 0 && raw.magnitude > 1)
            return move.normalized;
        return move;
    }

    public void RumbleStart(float lowFrequency, float highFrequency, float duration){
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null){
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            StartCoroutine(RumbleEnd(duration));
        }
    }

    private IEnumerator RumbleEnd(float duration){
        yield return new WaitForSecondsRealtime(duration);
        
        Gamepad gamepad = Gamepad.current;
        if (gamepad!=null){
            gamepad.SetMotorSpeeds(0, 0);
        }
    }

    // Change touch controls
    public static void StartRebindingListener(string actionName, Action<string> onRebindComplete)
    {
        // Seperate the actions in 2 arg if there exist
        string[] actionParts = actionName.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        string baseActionName = actionParts[0];
    
        string compositePart = actionParts.Length > 1 ? actionParts[1] : null;

    
        if (!actions.ContainsKey(baseActionName))
        {
            Debug.LogError("Action introuvable : " + baseActionName);
            return;
        }

        InputAction action = actions[baseActionName];

        
        if (!string.IsNullOrEmpty(compositePart))
        {
            int bindingIndex = -1;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                
                if (binding.isPartOfComposite && string.Equals(binding.name, compositePart, StringComparison.OrdinalIgnoreCase))
                {
                    bindingIndex = i;
                    break;
                }
            }

            if (bindingIndex == -1)
            {
                Debug.LogError($"Composite binding part '{compositePart}' non trouvé pour l'action '{baseActionName}'.");
                return;
            }

            Debug.Log($"Rebinding composite binding: {baseActionName} [{compositePart}] (index {bindingIndex})");
            action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    string newBinding = action.GetBindingDisplayString(bindingIndex);
                    onRebindComplete?.Invoke(newBinding);
                    Debug.Log($"Rebind terminé pour : {baseActionName} [{compositePart}] -> {newBinding}");
                })
                .Start();
        }
        else
        {
            Debug.Log($"Rebinding action: {baseActionName}");
            rebindingOperation = action.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse")
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    string newBinding = action.GetBindingDisplayString(0);
                    onRebindComplete?.Invoke(newBinding);
                    Debug.Log($"Rebind terminé pour : {baseActionName} -> {newBinding}");
                })
                .Start();
        }
    }

    public static void CancelRebinding()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Dispose();
            rebindingOperation = null;
            Debug.Log("Rebinding operation canceled.");
        }
    }

    public static void EnableGameplay()
    { 
        uiMap.Disable();
        gameplayMap.Enable();
        isUIActive = false;
        Debug.Log("Mode : Gameplay");
    }

    public static void ApplyRebind(string actionName, string newBinding)
    {
        if (actions.ContainsKey(actionName))
        {
            InputAction action = actions[actionName];

            action.ApplyBindingOverride(newBinding);
            Debug.Log("Rebind appliqué : " + actionName + " -> " + newBinding);
        }
    }


    public static void EnableUI()
    {
        if (gameplayMap == null || uiMap == null) return;
        
        gameplayMap.Disable();
        uiMap.Enable();
        isUIActive = true;
        Debug.Log("Mode : UI");
    }

    public static bool IsUIActive()
    {
        return isUIActive;
    }
}
