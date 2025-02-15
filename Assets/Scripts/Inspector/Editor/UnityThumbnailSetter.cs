using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(ItemData), true)]
public class UnityItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update to read the latest values

        SerializedProperty thumbnailProp = serializedObject.FindProperty("thumbnail");
        SerializedProperty nameProp = serializedObject.FindProperty("name");
        SerializedProperty categoryProp = serializedObject.FindProperty("category");
        SerializedProperty rarityProp = serializedObject.FindProperty("rarity");
        SerializedProperty priceProp = serializedObject.FindProperty("price");
        SerializedProperty meshProp = serializedObject.FindProperty("meshGroup");

        // HEADER
        GUILayout.BeginHorizontal();

        // Display sprite
        Sprite thumbnail = (Sprite)thumbnailProp.objectReferenceValue;
        if (thumbnail != null)
        {
            GUILayout.Label(thumbnail.texture, GUILayout.Width(100), GUILayout.Height(100));
        }
        else
        {
            GUILayout.Label("No Thumbnail", GUILayout.Width(100), GUILayout.Height(100));
        }
        
        // Sprite picker
        EditorGUILayout.PropertyField(thumbnailProp, GUIContent.none, GUILayout.Width(20), GUILayout.Height(100));


        EditorGUILayout.Space();

        GUILayout.BeginVertical();
        EditorGUILayout.PropertyField(nameProp, GUIContent.none);
        EditorGUILayout.PropertyField(categoryProp);
        EditorGUILayout.PropertyField(rarityProp);
        EditorGUILayout.PropertyField(priceProp);
        EditorGUILayout.PropertyField(meshProp);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        // HEADER END
        
        // Manually draw the remaining properties (causes conflict otherwise)
        string[] hide = {"m_Script", "thumbnail", "name", "category", "rarity", "meshGroup", "price"};
        
        SerializedProperty iterator = serializedObject.GetIterator();
        bool first = true;
        while (iterator.NextVisible(first))
        {
            if (!hide.Contains(iterator.name)) // Skip hidden fields
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            first = false;
        }
        
        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
