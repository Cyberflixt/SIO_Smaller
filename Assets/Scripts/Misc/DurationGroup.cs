using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DurationGroup
{
    public bool empty{
        get { return group.Count == 0; }
    }


    private List<float> group = new List<float>();

    public void Add(float value = 1)
    {
        if (value > 0)
            group.Add(value);
    }

    public void Clear(){
        group = new List<float>();
    }

    public bool Update()
    {
        float delta = Time.deltaTime;
        for (int i = 0; i < group.Count; i++){
            float v = group[i];
            if (v > delta){
                group[i] = v-delta;
            } else {
                group.RemoveAt(i);
                i--;
            }
        }
        return empty;
    }
}
