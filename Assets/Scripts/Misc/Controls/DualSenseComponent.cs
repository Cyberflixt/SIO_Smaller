using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniSense;
using UnityEngine.InputSystem;

public abstract class DualSenseComponent : MonoBehaviour
{
    private void Start()
    {
        var dualSense = UniSense.DualSenseGamepadHID.FindCurrent();
        var isDualSenseConected = dualSense != null;
        if (isDualSenseConected) OnConnect(dualSense);
        else OnDisconnect();
    }

    UniSense.DualSenseGamepadHID dualSense;

    private void OnEnable() => InputSystem.onDeviceChange += OnDeviceChange;

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        OnDisconnect();

        // Reset controller
        DualSenseGamepadHID dualSense = UniSense.DualSenseGamepadHID.FindCurrent();
        dualSense?.Reset();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        var isNotDualSense = !(device is UniSense.DualSenseGamepadHID);
        if (isNotDualSense) return;

        switch (change)
        {
            case InputDeviceChange.Added:
                OnConnect(device as UniSense.DualSenseGamepadHID);
                dualSense = device as UniSense.DualSenseGamepadHID;
                break;
            case InputDeviceChange.Reconnected:
                OnConnect(device as UniSense.DualSenseGamepadHID);
                dualSense = device as UniSense.DualSenseGamepadHID;
                break;
            case InputDeviceChange.Disconnected:
                OnDisconnect();
                dualSense = device as UniSense.DualSenseGamepadHID;
                break;
        }
    }

    internal virtual void OnConnect(UniSense.DualSenseGamepadHID dualSense){}
    internal virtual void OnDisconnect(){}
}
