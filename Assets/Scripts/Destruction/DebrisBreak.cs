using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebrisBreak
{
    public static void Spawn(Transform mesh){
        // Spawn vfx on a given mesh
        Renderer renderer = mesh.GetComponent<Renderer>();
        if (renderer){
            SpawnRenderer(mesh, renderer);
        } else {
            foreach(Transform child in mesh)
                Spawn(child);
        }
    }

    public static void SpawnRenderer(Transform mesh, Renderer renderer){
        // Neutral
        Spawn_vfx(mesh, DebrisBreakData.instance.vfx_neutral);

        // Each material
        foreach (Material mat in renderer.sharedMaterials){
            Spawn_vfx(mesh, Get_material_vfx(mat));
        }
    }

    private static Transform Get_material_vfx(Material mat){
        if (DebrisBreakData.instance.vfx_spe_materials.ContainsKey(mat))
            return DebrisBreakData.instance.vfx_spe_materials[mat];
        
        Transform vfx = DebrisBreakData.instance.vfx_material;
        ParticleSystemRenderer ps = vfx.GetComponent<ParticleSystemRenderer>();
        ps.material = mat;
        return vfx;
    }

    private static void Spawn_vfx(Transform mesh, Transform vfx){
        Vector3 center = mesh.position;

        Transform instance = GameObject.Instantiate(vfx);
        instance.position = center;

        GameObject.Destroy(instance.gameObject, 10);
    }
}
