using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TransformList{
    public List<Transform> list;
}

[CreateAssetMenu(fileName = "DestructibleWall_", menuName = "Destructible/DestructibleWallData", order = 1)]
public class DestructibleWallData : ScriptableObject 
{
    public Vector3 cellSize = new Vector3(1,1,1);
    public List<TransformList> meshes = new List<TransformList>();
    public bool mesh1RandomRotation = false;
    public int addedRotation = 0;

    public Transform RandomMesh(int i){
        // Returns a random mesh inside the list of given index
        List<Transform> li = meshes[i].list;

        if (li.Count == 0) return null;
        return li[Random.Range(0, li.Count-1)];
    }
    public Transform InstantiateRandomMesh(int i){
        // Get random mesh, instantiate it
        Transform mesh = RandomMesh(i);

        if (mesh){
            return Instantiate(mesh);
        }
        return null;
    }

    public bool IsFilled(){
        // Is the data not sufficient
        return meshes.Count>6;
    }
}
