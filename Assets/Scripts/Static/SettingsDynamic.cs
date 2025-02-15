using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingsDynamic
{
    // Field of view
    public static float fovDefault = 70;
    public static FloatGroup fovFactors = new FloatGroup();
    public static float GetFov(){
        return fovDefault * fovFactors.GetProduct();
    }


    // Mouse sensibility
    public static float sensibilityDefault = 4f;
    public static float sensibilityAimFactor = .5f;
    public static FloatGroup sensibilityFactors = new FloatGroup();
    public static float GetSensibility(){
        return sensibilityDefault * sensibilityFactors.GetMinimum();
    }
}
