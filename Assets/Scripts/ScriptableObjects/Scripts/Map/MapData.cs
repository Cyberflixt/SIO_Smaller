using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

[CreateAssetMenu(fileName = "Map_", menuName = "Map/MapData", order = 1)]
public class MapData : ScriptableObject
{
    public new string name = "Unnamed map";
    public string sceneName = "Unnamed scene";
}
