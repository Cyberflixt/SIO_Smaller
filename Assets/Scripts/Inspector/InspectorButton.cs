using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;


/// <summary>
/// Adds a button to the following function
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public class ButtonAttribute : Attribute
{
    public string name = null;
    public ButtonAttribute(string text)
    {
        name = text;
    }
}

// Pain and suffering
#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonAttributeEditor : Editor {
    public override void OnInspectorGUI () {
        DrawDefaultInspector();

        MonoBehaviour monoBehaviour = (MonoBehaviour)target;

        Type type = monoBehaviour.GetType();
        
        // Loop through all methods in the class
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            // Check if the method has the InspectorButton attribute
            ButtonAttribute[] attributes = (ButtonAttribute[]) method.GetCustomAttributes(typeof(ButtonAttribute), true);
            if (attributes.Length > 0){
                // Show button with name of the attribute / or the method name
                ButtonAttribute data = attributes[0];
                string name = data.name == null ? method.Name : data.name;

                if (GUILayout.Button(name)){
                    // Clicked, call the method
                    method.Invoke(monoBehaviour, null);
                }
            }
        }
   }
}
#endif
