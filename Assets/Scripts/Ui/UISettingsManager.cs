using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[System.Serializable]
public class Tab
{
    public GameObject tabButton;
    public GameObject tabPanel;
}
public class UISettingsManager : MonoBehaviour
{
    public static UISettingsManager instance = null;
    [Header("Tabs")]
    [SerializeField] private List<Tab> settingsTabs;
    private int activeTabIndex = 1;
    [SerializeField] private Color activeColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.3f);

    [SerializeField] private GameObject TouchListener;

    [SerializeField] private GameObject pausedMenu;

    [Header("Game Main Object")]
    public Light mainLight;
    public UniversalRenderPipelineAsset urpAsset;
    public InputActionMap playerControls;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    


    [Header("UI Elements")]

    [SerializeField] private GameObject[] TouchControls;

    [SerializeField] private UI_Slider renderScaleSlider;
    [SerializeField] private UI_Slider shadowSlider;
    [SerializeField] private UI_Slider antiAliasingSlider;
    [SerializeField] private UI_Slider LODSlider;
    
    [SerializeField] private UI_Slider presetSlider;
    [SerializeField] private UI_Slider screenModeSlider; 
    [SerializeField] private UI_Slider FPS; 

    [SerializeField] private UI_SliderRange FOV; 
    [SerializeField] private UI_SliderRange sensibility;


    [SerializeField] private Button applyButton;

    private int savedPreset;
    private int savedRenderScale;
    private int savedShadow;
    private int saveAntialiasing;
    private int savedLOD;
    private bool savedPostProcessing;
    private bool savedOutlineEffect;
    private bool savedAmbientOcclusion;
    private bool savedDecals;

    private int savedScreenMode = 0;
    private int savedFPS = 1;

    public bool isActivated = false;

    private void Start()
    {
        instance = this;

        // LoadRebinds(); Uncomment if you want to load touch controls on restart
        for (int i = 0; i < settingsTabs.Count; i++)
        {
            int index = i;
            settingsTabs[i].tabButton.GetComponent<Button>().onClick.AddListener(() => OnTabClicked(index));
            SetTabColor(settingsTabs[i].tabButton, inactiveColor);
        }
        OnTabClicked(0);
        LoadSettings();
        screenModeSlider.OnOptionChanged += SettingsChanged;
        FPS.OnOptionChanged += SettingsChanged;

        presetSlider.OnOptionChanged += PresetChanged;
        
        renderScaleSlider.OnOptionChanged += QualityChanged;
        shadowSlider.OnOptionChanged += QualityChanged;
        antiAliasingSlider.OnOptionChanged += QualityChanged;
        LODSlider.OnOptionChanged += QualityChanged;

        FOV.OnOptionChanged += SettingsChanged;
        sensibility.OnOptionChanged += SettingsChanged;

        applyButton.gameObject.SetActive(false);
    }

    public void TouchControlsClick(GameObject touchUI)
    {
        string actionName = touchUI.GetComponentInChildren<TextMeshProUGUI>().text;
        TouchListener.SetActive(true);

    
        void UpdateUI(string newBinding)
        {
            touchUI.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = newBinding;
            TouchListener.SetActive(false);
            SaveRebind(actionName, newBinding);
        }
        InputExt.StartRebindingListener(actionName, UpdateUI);
    }


    

    private void SaveRebind(string actionName, string newBinding)
    {
        PlayerPrefs.SetString("Rebind_" + actionName, newBinding);
        PlayerPrefs.Save();
        Debug.Log("Touch saved" + actionName + " -> " + newBinding);
    }

        private void LoadRebinds()
    {
        foreach (GameObject touchControl in TouchControls)
        {
            string actionName = touchControl.GetComponentInChildren<TextMeshProUGUI>().text;

            if (PlayerPrefs.HasKey("Rebind_" + actionName))
            {
                string savedBinding = PlayerPrefs.GetString("Rebind_" + actionName);
                touchControl.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = savedBinding;

                InputExt.ApplyRebind(actionName, savedBinding);
            }
        }
        Debug.Log("Touch are loaded");
    }


    public void CancelRebinding()
    {
        InputExt.CancelRebinding();
        TouchListener.SetActive(false);
    }

    private void SettingsChanged()
    {
        applyButton.gameObject.SetActive(true);
    }


    private void QualityChanged()
    {
        presetSlider.SetCurrentIndex(3);
        applyButton.gameObject.SetActive(true);
    }

    private void PresetChanged()
    {
        if (presetSlider.GetCurrentIndex() == 3)
        {
            return;
        } else if( presetSlider.GetCurrentIndex() == 0)
        {
            renderScaleSlider.SetCurrentIndex(0);
            shadowSlider.SetCurrentIndex(1);
            antiAliasingSlider.SetCurrentIndex(0);
            LODSlider.SetCurrentIndex(0);
        } else if( presetSlider.GetCurrentIndex() == 1)
        {
            renderScaleSlider.SetCurrentIndex(1);
            shadowSlider.SetCurrentIndex(2);
            antiAliasingSlider.SetCurrentIndex(1);
            LODSlider.SetCurrentIndex(1);
        } else if( presetSlider.GetCurrentIndex() == 2)
        {
            renderScaleSlider.SetCurrentIndex(2);
            shadowSlider.SetCurrentIndex(3);
            antiAliasingSlider.SetCurrentIndex(2);
            LODSlider.SetCurrentIndex(2);
        }
        applyButton.gameObject.SetActive(true);
    }

    public void OpenSettings()
    {
        pausedMenu.SetActive(false);
        LoadSettings();
        isActivated = true;
        UI_Main.SetActiveUI("Settings");
    }

    public void CloseSettings()
    {
        isActivated = false;
        UI_Main.SetActiveUI("PauseMenu");
    }


    public void OnTabClicked(int tabIndex)
    {
        if (tabIndex == activeTabIndex) return;


        settingsTabs[activeTabIndex].tabPanel.SetActive(false);
        SetTabColor(settingsTabs[activeTabIndex].tabButton, inactiveColor);

        Debug.Log("Tab clicked: " + tabIndex);
        activeTabIndex = tabIndex;
        settingsTabs[activeTabIndex].tabPanel.SetActive(true);
        SetTabColor(settingsTabs[activeTabIndex].tabButton, activeColor);
    }

        private void SetTabColor(GameObject tabButton, Color color)
    {
        TextMeshProUGUI buttonText = tabButton.GetComponent<TextMeshProUGUI>();
        buttonText.color = color;
    }



    public void ApplyChanges()
    {
        savedScreenMode = screenModeSlider.GetCurrentIndex();
        savedFPS = FPS.GetCurrentIndex();
        savedRenderScale = renderScaleSlider.GetCurrentIndex();
        savedShadow = shadowSlider.GetCurrentIndex();
        saveAntialiasing = antiAliasingSlider.GetCurrentIndex();
        savedLOD = LODSlider.GetCurrentIndex();
        savedPreset = presetSlider.GetCurrentIndex();

        Screen.fullScreenMode = GetFullScreenModeFromIndex(savedScreenMode);
        Application.targetFrameRate = GetFPSFromIndex(savedScreenMode);
        RenderSettings.skybox.SetFloat("_Exposure", GetRenderScale(savedRenderScale));


        QualitySettings.shadowResolution =  GetShadow(savedShadow).Item1;
        mainLight.shadows =  GetShadow(savedShadow).Item2;
        QualitySettings.shadowDistance = GetShadow(savedShadow).Item3;

        QualitySettings.antiAliasing = GetAntiAliasing(saveAntialiasing);
        QualitySettings.lodBias = GetLOD(savedLOD);

        SettingsDynamic.fovDefault = FOV.GetValue();
        SettingsDynamic.sensibilityDefault = sensibility.GetValue();

    
        PlayerPrefs.SetInt("ScreenMode", savedScreenMode);
        PlayerPrefs.SetInt("FPS", savedFPS);

        PlayerPrefs.SetFloat("FOV", FOV.GetValue());
        PlayerPrefs.SetFloat("Sensibility", sensibility.GetValue());

        PlayerPrefs.SetInt("Preset", savedPreset);
        PlayerPrefs.SetInt("RenderScale", savedRenderScale);
        PlayerPrefs.SetInt("Shadow", savedShadow);
        PlayerPrefs.SetInt("AntiAliasing", saveAntialiasing);
        PlayerPrefs.SetInt("LOD", savedLOD);

        PlayerPrefs.Save();
        applyButton.gameObject.SetActive(false);
    }

    private void LoadSettings()
    {
        savedScreenMode = PlayerPrefs.GetInt("ScreenMode", 0);
        savedFPS = PlayerPrefs.GetInt("FPS", 1);
        savedRenderScale = PlayerPrefs.GetInt("RenderScale", 1);
        savedShadow = PlayerPrefs.GetInt("Shadow", 1);
        saveAntialiasing = PlayerPrefs.GetInt("AntiAliasing", 1);
        savedLOD = PlayerPrefs.GetInt("LOD", 1);


        // LOAD UI
        screenModeSlider.SetCurrentIndex(savedScreenMode);
        FPS.SetCurrentIndex(savedFPS);
        renderScaleSlider.SetCurrentIndex(savedRenderScale);
        shadowSlider.SetCurrentIndex(savedShadow);
        antiAliasingSlider.SetCurrentIndex(saveAntialiasing);
        LODSlider.SetCurrentIndex(savedLOD);

        FOV.SetValue(PlayerPrefs.GetFloat("FOV", 70.0f));
        sensibility.SetValue(PlayerPrefs.GetFloat("Sensibility", 1.0f));

        Debug.Log("Fps Load: " + GetFPSFromIndex(savedFPS));

        Screen.fullScreenMode = GetFullScreenModeFromIndex(savedScreenMode);
        Application.targetFrameRate = GetFPSFromIndex(savedFPS);
        RenderSettings.skybox.SetFloat("_Exposure", GetRenderScale(savedRenderScale));

        QualitySettings.shadowResolution =  GetShadow(savedShadow).Item1;
        mainLight.shadows =  GetShadow(savedShadow).Item2;
        QualitySettings.shadowDistance = GetShadow(savedShadow).Item3;

        QualitySettings.antiAliasing = GetAntiAliasing(saveAntialiasing);
        QualitySettings.lodBias = GetLOD(savedLOD);
    }
    private FullScreenMode GetFullScreenModeFromIndex(int index)
    {
        switch (index)
        {
            case 0: return FullScreenMode.ExclusiveFullScreen; 
            case 1: return FullScreenMode.Windowed;            
            case 2: return FullScreenMode.FullScreenWindow;    
            default: return FullScreenMode.Windowed;
        }
    }

    private int GetFPSFromIndex(int index)
    {
        switch (index)
        {
            case 0: return 30; 
            case 1: return 60;            
            case 2: return 144;
            case 3: return 244;     
            default: return 60;
        }
    }

    private float GetRenderScale(int index)
    {
        switch (index)
        {
            case 0: return 0.5f; 
            case 1: return 1.0f;            
            case 2: return 1.5f;    
            default: return 1;
        }
    }
    private (UnityEngine.ShadowResolution, LightShadows, float) GetShadow(int index) 
    {
        switch (index)
        {
            case 0:
                return (UnityEngine.ShadowResolution.Low, LightShadows.None, 25.0f);
            case 1:
                return (UnityEngine.ShadowResolution.Medium, LightShadows.Hard, 50.0f);
            case 2:
                return (UnityEngine.ShadowResolution.High, LightShadows.Hard, 100.0f);
            case 3:
                return (UnityEngine.ShadowResolution.VeryHigh, LightShadows.Soft, 100.0f);
            default:
                return (UnityEngine.ShadowResolution.Medium, LightShadows.Hard, 50.0f);
        }
    }
    private int GetAntiAliasing (int index)
    {
        switch (index)
        {
            case 0: return 0; 
            case 1: return 2;            
            case 2: return 8;    
            default: return 2;
        }
    }
    private int GetLOD (int index)
    {
        switch (index)
        {
            case 0: return 0; 
            case 1: return 1;            
            case 2: return 2;    
            default: return 1;
        }
    }

}
