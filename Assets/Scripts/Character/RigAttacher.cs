using System.Collections.Generic;
using UnityEngine;

// static classes my beloved

public static class RigAttacher
{
    // u/pixels
    public static void Attach(Transform target, Transform source)
    {
        // create rig dictionnary <name, bone>
        SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>();
        Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();

        foreach (Transform bone in targetRenderer.bones) {
            targetBones[bone.gameObject.name] = bone;
        }

        // create newBones bone array
        SkinnedMeshRenderer sourceRenderer = source.GetComponent<SkinnedMeshRenderer>();
        Transform[] newBones = new Transform[sourceRenderer.bones.Length];

        for (int i = 0; i < sourceRenderer.bones.Length; ++i)
        {
            GameObject bone = sourceRenderer.bones[i].gameObject;
            if (!targetBones.TryGetValue(bone.name, out newBones[i]))
            {
                Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
                break;
            }
        }

        // swap sourceBones to targetBones
        sourceRenderer.bones = newBones;
    }

    public static Transform AttachModelPath(Transform target, string path)
    {
        Transform file = Resources.Load<Transform>(path);
        Transform obj = GameObject.Instantiate(file);
        obj.parent = target;
        obj.localPosition = Vector3.zero;

        SkinnedMeshRenderer[] all = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mod in all)
        {
            Attach(target, mod.transform);
        }

        return obj;
    }

    public static Transform AttachEquipment(Transform target, string path)
    {
        Transform newobj = AttachModelPath(target, path);

        int layer = target.gameObject.layer;
        SetLayerRecursively(newobj, layer);
        return newobj;
    }

    public static void SetLayerRecursively(Transform obj, int newLayer)
    {
        obj.gameObject.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child, newLayer);
        }
    }
}
