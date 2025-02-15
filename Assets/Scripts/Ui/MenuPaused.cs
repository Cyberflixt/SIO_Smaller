using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuPaused : MonoBehaviour
{
    public static MenuPaused instance;
    public VolumeProfile volumePaused;
    private VolumeProfile volumeDefault;

    
    private void Awake()
    {
        instance = this;
        volumeDefault = GetVolume().profile;
    }

    private Volume GetVolume(){
        Transform mapData = GameScenes.GetMapData();
        Volume volume = mapData.GetComponentInChildren<Volume>();
        return volume;
    }

    public void PauseGame()
    {
        // Enable UI
        UI_Main.SetActiveUI("PauseMenu");

        // Vfx
        Volume volume = GetVolume();
        if (volume){
            volume.profile = volumePaused;
        }

        // Disable controls
        InputExt.EnableUI();
        if (CharControls.instance)
            CharControls.instance.enabled = false;
        if (InventoryControls.instance)
            InventoryControls.instance.enabled = false;
        
        // Unlock mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        // Disable UI
        UI_Main.SetActiveUI("Gameplay");

        // Stop vfx
        Volume volume = GetVolume();
        if (volume){
            volume.profile = volumeDefault;
        }
        
        // Enable controls
        InputExt.EnableGameplay();
        if (CharControls.instance)
            CharControls.instance.enabled = true;
        if (InventoryControls.instance)
            InventoryControls.instance.enabled = true;

        Cursor.visible = false;
    }

    public void Click()
    {
        Debug.Log("Click");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        SceneManager.LoadScene("MainMenu");
    }
}
