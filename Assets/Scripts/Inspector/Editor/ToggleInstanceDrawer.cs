using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// Draws a button to toggle between the default value of a class or null
/// </summary>
[CustomPropertyDrawer(typeof(ToggleInstanceAttribute))]
public class ToggleInstanceDrawer : PropertyDrawer
{
#if UNITY_EDITOR
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Default size
        float height = EditorGUIUtility.singleLineHeight;

        // Add the size of children if dropdown property
        if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue != null)
            height += EditorGUI.GetPropertyHeight(property, true) - EditorGUIUtility.singleLineHeight;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        bool isUnityObject = property.propertyType == SerializedPropertyType.ObjectReference;
        bool isManagedReference = property.propertyType == SerializedPropertyType.ManagedReference;
        bool isNull = isUnityObject ? property.objectReferenceValue == null : property.managedReferenceValue == null;

        // Get btn width (small if not null)
        float btnWidth = 60;
        if (!isNull)
            btnWidth = EditorGUIUtility.singleLineHeight * .9f;

        // Draw normal property
        Rect fieldRect = new Rect(position.x, position.y, position.width - btnWidth, EditorGUIUtility.singleLineHeight);
        if (isManagedReference && property.managedReferenceValue != null){
            EditorGUI.PropertyField(fieldRect, property, label, true);
        } else {
            EditorGUI.PropertyField(fieldRect, property, label);
        }

        // Draw the button
        Rect buttonRect = new Rect(fieldRect.xMax, position.y, btnWidth, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(buttonRect, isNull ? "New" : "X"))
        {
            // Button pressed, toggle between null / constructor
            Type fieldType = fieldInfo.FieldType;

            if (isUnityObject)
            {
                if (isNull)
                {
                    // Set to default constructor
                    if (typeof(ScriptableObject).IsAssignableFrom(fieldType))
                        property.objectReferenceValue = ScriptableObject.CreateInstance(fieldType);
                } else {
                    // Set to null
                    property.objectReferenceValue = null;
                }
            }
            else if (isManagedReference) // Serialized ref (pure c# classes)
            {
                // Toggle
                property.managedReferenceValue = isNull ? Activator.CreateInstance(fieldType) : null;
            }
            else // Peak unity
            {
                // Toggle
                object instance = isNull ? Activator.CreateInstance(fieldType) : null;

                property.serializedObject.targetObject.GetType()
                    .GetField(property.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(property.serializedObject.targetObject, instance);
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
#endif
}
