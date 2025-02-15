using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UniSense;
using Cinemachine;

public class ScreenShake : MonoBehaviour
{
    struct ShakeData{
        public Vector3 pos;
        public float strength;
        public float duration;
        public float time;
        public ShakeData(Vector3 _pos, float _strength, float _duration, float _time){
            pos = _pos;
            strength = _strength;
            duration = _duration;
            time = _time;
        }
    }

    private float amplitude = 3f;

    static List<ShakeData> shakes = new List<ShakeData>();
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin cameraNoise;

    void Start(){
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        cameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin> ();
    }

    void LateUpdate(){
        // Update shaking strength
        float max = 0;

        if (Camera.main != null){
            Vector3 camera = Camera.main.transform.position;

            // Sum of all the shakes
            for(int i = 0; i<shakes.Count; i++){
                ShakeData data = shakes[i];
                float alpha = (Time.time - data.time) / data.duration;
                if (alpha > 0){
                    if (alpha < 1){
                        // Add to total force
                        float dist = Utils.SlowSlope((data.pos - camera).sqrMagnitude, 100000) * 100 + 1;

                        float time_falloff = (1-alpha) * (1-alpha);
                        float strength = data.strength * time_falloff / dist;
                        if (strength > max)
                            max = strength;
                    } else {
                        // Delete shake data
                        shakes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        
        // Apply shaking
        float amp = Utils.SlowSlope(max, 20) * amplitude;
        cameraNoise.m_AmplitudeGain = amp;

        // Controller rumbling
        float rumbling_factor = .1f;
        float rumbling = Mathf.Clamp01(amp * rumbling_factor - .1f);

        DualSenseControls.SetMotorSpeeds(0, rumbling);
        //Testing();
    }
    public static void ShakeStart(Vector3 pos, float strength, float duration){
        shakes.Add(new ShakeData(pos, strength, duration, Time.time));
        //InputExt.instance.RumbleStart(.5f,1f,.25f);
    }
    public static void ShakeStart(Vector3 pos, float strength, float duration, float time){
        shakes.Add(new ShakeData(pos, strength, duration, time));
        //InputExt.instance.RumbleStart(.5f,1f,.25f);
    }

    static bool test = false;
    static float tick = 0;
    void Testing(){
        // Controller  testing, hold square for test
        if (InputExt.actions["Test"].IsPressed()){
            if (!test){
                tick = Time.time;
                test = true;
                DualSenseControls.SetLightBarColor(UnityEngine.Random.ColorHSV(0,1,1,1,1,1));
                DualSenseControls.SetTriggerEffects(new DualSenseTriggerState
                    {
                        EffectType = DualSenseTriggerEffectType.SectionResistance,
                        Section = new DualSenseSectionResistanceProperties(){
                            StartPosition = 10,
                            EndPosition = 50,
                            Force = 255
                        }
                    },
                    new DualSenseTriggerState
                    {
                        EffectType = DualSenseTriggerEffectType.EffectEx,
                        EffectEx = new DualSenseEffectExProperties(){
                            KeepEffect = true,
                            StartPosition = 0,
                            BeginForce = (byte)(255),
                            MiddleForce = (byte)(255),
                            EndForce = (byte)(255),
                            Frequency = (byte)(10),
                        },
                    }
                );
            }
            if (InputExt.actions["R"].ReadValue<float>() > .9f){
                float alpha = (Time.time - tick)/3;
                float beta = (Time.time - tick)/3-.5f;
                DualSenseControls.SetMotorSpeeds(beta, alpha);
            } else {
                DualSenseControls.SetMotorSpeeds(0, 0);
                tick = Time.time;
            }
        } else {
            // Reset
            if (test){
                test = false;
                DualSenseControls.SetLightBarColor(UnityEngine.Random.ColorHSV(0,1,1,1,1,1));
                DualSenseControls.TriggerResetEffects();
            }
            DualSenseControls.SetMotorSpeeds(0, 0);
        }
    }
}
