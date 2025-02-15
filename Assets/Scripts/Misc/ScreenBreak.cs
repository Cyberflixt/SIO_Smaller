using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreenBreak : MonoBehaviour
{
    public Transform mesh;
    public Transform corner;
    public Material brokenMaterial;
    public Texture[] brokenTextures;
    private int oldTextureInt = 0;
    
    void Update()
    {
        Break(transform.position);
    }

    public void Break(Vector3 pos){
        // Project hit in UV coords
        Vector3 halfSize = corner.localPosition * 2;
        Vector3 proj = mesh.InverseTransformPoint(pos);
        Vector2 flat = new Vector2(proj.x / halfSize.x, -proj.y / halfSize.y);

        // Apply material
        int materialIndex = 1;
        MeshRenderer ren = mesh.GetComponent<MeshRenderer>();
        Material[] materials = ren.materials;
        materials[materialIndex] = brokenMaterial;
        ren.SetMaterials(materials.ToList());

        // Random different texture
        int ran = UnityEngine.Random.Range(0, brokenTextures.Length-1);
        if (ran >= oldTextureInt) ran++;
        oldTextureInt = ran;
        ren.materials[materialIndex].SetTexture("_Image", brokenTextures[ran]);
        ren.materials[materialIndex].SetVector("_Center", flat);
    }
}
