
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
#if UNITY_EDITOR
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute condHAtt = (ShowIfAttribute)attribute;
        bool enabled = GetConditionalSourceValue(property, condHAtt);
        //GUI.enabled = enabled;

        if (enabled){
            // Enabled, normal
            EditorGUI.PropertyField(position, property, label, true);
        }
        // else, hidden
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute target = (ShowIfAttribute)attribute;
        bool enabled = GetConditionalSourceValue(property, target);

        // if is enable draw the label
        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            // Hide
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    /// <summary>
    /// Get if the conditional what expected is true.
    /// </summary>
    /// <param name="property"> is used for get the value of the property and check if return enable true or false </param>
    /// <param name="target"> is the attribute what contains the values what we need </param>
    /// <returns> only if the field y is same to the value expected return true</returns>
    private bool GetConditionalSourceValue(SerializedProperty property, ShowIfAttribute target)
    {
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, target.key);
        SerializedProperty propertyValue = property.serializedObject.FindProperty(conditionPath);

        // Found serialized field?
        if (propertyValue != null)
        {
            return propertyValue.boolValue == target.value;
        }

        // Find property using reflection
        UnityEngine.Object targetObject = property.serializedObject.targetObject;
        Type targetType = targetObject.GetType();
        PropertyInfo propertyInfo = targetType.GetProperty(target.key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // Found bool property?
        if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
        {
            return (bool)propertyInfo.GetValue(targetObject) == target.value;
        }

        // Neither field nor property found
        Debug.LogWarning($"ShowIf: Property or field '{target.key}' not found on {targetType}");
        return false;
    }

#endif
}