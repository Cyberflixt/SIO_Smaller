using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CharCamera : MonoBehaviour
{
    // Public
    public CharControls charControls;
    public CinemachineVirtualCamera virtualCamera;
    [HideInInspector] public float fovCurrent = 70;
    [HideInInspector] public static float fovAimingFactor = .5f;

    // Private
    private Cinemachine3rdPersonFollow cine_body;
    private CinemachineComposer cine_aim;
    private Vector3 rotation = new Vector3();

    // Panning
    public BoolGroup panSide = new BoolGroup();
    private float pan_value = .5f;
    private const float pan_speed = 5;
    private readonly Vector3 offset_default = new Vector3(0, .3f, 0);
    private Vector3 offset_aiming = new Vector3(.4f, .5f, 0);


    private void UpdateFOV(){
        // Get desired field of view
        float fovTarget;
        if (charControls.charAttacks.aiming){
            fovTarget = SettingsDynamic.fovDefault * fovAimingFactor;
        } else {
            fovTarget = SettingsDynamic.GetFov();
        }

        // Lerp field of view
        fovCurrent = Mathf.Lerp(fovCurrent, fovTarget, Mathf.Clamp01(Time.deltaTime * 5f));
        virtualCamera.m_Lens.FieldOfView = fovCurrent;
    }

    public void UpdateCameraCharacter(){
        // Camera rotation
        float sensibility = SettingsDynamic.GetSensibility() * fovCurrent / SettingsDynamic.fovDefault;
        Vector2 delta = InputExt.actions["MouseDelta"].ReadValue<Vector2>();
        delta *= sensibility;
        rotation += new Vector3(-delta.y, delta.x, 0);

        // Clamping rotation
        float maxPitch = 50;
        rotation = new Vector3(Mathf.Clamp(rotation.x, -maxPitch, maxPitch), rotation.y, 0);
        transform.eulerAngles = rotation;

        // Camera on the side
        pan_value = Mathf.Lerp(pan_value, panSide ? 1 : 0, Time.deltaTime * pan_speed);
        Vector3 offset = Vector3.Lerp(offset_default, offset_aiming, pan_value);

        cine_body.ShoulderOffset = offset;
        cine_aim.m_TrackedObjectOffset = offset;
        
        UpdateFOV();
    }

    public void UpdateCameraDead(){
        // Camera rotation
        Vector2 delta = InputExt.actions["MouseDelta"].ReadValue<Vector2>();
        delta *= SettingsDynamic.GetSensibility();
        rotation += new Vector3(-delta.y, delta.x, 0);

        // Clamping rotation
        float maxPitch = 50;
        rotation = new Vector3(Mathf.Clamp(rotation.x, -maxPitch, maxPitch), rotation.y, 0);
        transform.eulerAngles = rotation;

        // Camera on the side
        pan_value = Mathf.Lerp(pan_value, panSide ? 1 : 0, Time.deltaTime * pan_speed);
        Vector3 offset = Vector3.Lerp(offset_default, offset_aiming, pan_value);

        cine_body.ShoulderOffset = offset;
        cine_aim.m_TrackedObjectOffset = offset;
        
        UpdateFOV();
    }

    #region Events

    private void Awake(){
        cine_body = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        cine_aim = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
    }
    void OnEnable(){
        virtualCamera.gameObject.SetActive(true);
    }
    void OnDisable(){
        virtualCamera.gameObject.SetActive(false);
    }

    #endregion
}
