using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UniSense;

public class DualSenseControls : DualSenseComponent
{
    // DualSenseComponent events
    public static UniSense.DualSenseGamepadHID controller;

    // Saved properties
    static Color _color = Color.blue;
    static DualSenseGamepadState _state = new DualSenseGamepadState();

    internal override void OnConnect(UniSense.DualSenseGamepadHID dualSense){
        controller = dualSense;
        controller.SetLightBarColor(_color);
    }

    internal override void OnDisconnect() => controller = null;

    // Methods
    public static void SetLightBarColor(Color color){
        _color = color;
        _state.LightBarColor = color;
        controller?.SetLightBarColor(color);
    }

    public static void TriggerResetEffects(){
        _state.LeftTrigger = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.NoResistance,
        };
        _state.RightTrigger = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.NoResistance,
        };
    }
    public static void SetTriggerEffects(DualSenseTriggerState leftTrigger, DualSenseTriggerState rightTrigger){
        _state.LeftTrigger = leftTrigger;
        _state.RightTrigger = rightTrigger;
    }
    public static void SetGamepadState(DualSenseGamepadState state){
        _state = state;
    }

    public static void SetMotorSpeeds(float lowFrequencyMotorSpeed, float highFrequenceyMotorSpeed){
        _state.Motor = new DualSenseMotorSpeed(lowFrequencyMotorSpeed, highFrequenceyMotorSpeed);
        Gamepad.current?.SetMotorSpeeds(lowFrequencyMotorSpeed, highFrequenceyMotorSpeed);
    }

    void Update(){
        if (controller != null){
            controller.SetGamepadState(_state);
        }
    }
}