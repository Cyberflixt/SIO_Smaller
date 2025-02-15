using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxFloat))]
[CustomPropertyDrawer(typeof(MinMaxInt))]
[CanEditMultipleObjects]
public class MinMaxValuesEditor : PropertyDrawer
{
#if UNITY_EDITOR
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child indented
        int  indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        float half = position.width / 2;
        Rect rect0 = new Rect(position.x, position.y, half, position.height);
        Rect rect1 = new Rect(position.x + half, position.y, half, position.height);

        Rect label0 = new Rect(rect0.x, position.y, 50, position.height);
        Rect label1 = new Rect(rect1.x, position.y, 50, position.height);

        Rect prop0 = new Rect(rect0.x+30, rect0.y, rect0.width - 32, rect0.height);
        Rect prop1 = new Rect(rect1.x+30, rect1.y, rect1.width - 32, rect1.height);

        // PrefixLabel first, then property
        EditorGUI.PrefixLabel(label0, new GUIContent("Min"));
        EditorGUI.PropertyField(prop0, property.FindPropertyRelative("min"), GUIContent.none);
        EditorGUI.PrefixLabel(label1, new GUIContent("Max"));
        EditorGUI.PropertyField(prop1, property.FindPropertyRelative("max"), GUIContent.none);

        // Set indent back to previous value
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
#endif
}