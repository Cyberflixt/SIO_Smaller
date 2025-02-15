
using UnityEngine;
using UnityEditor;

/// <summary>
/// Atttribute for show a field if other field is true or false.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public class ShowIfAttribute : PropertyAttribute
{
    public string key;
    public bool value;
    public bool hide;

    public ShowIfAttribute(string key, bool value = true)
    {
        this.key = key;
        this.value = value;
    }
}