using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Group of float, can return a minimum, maximum, sum, etc...
/// </summary>
public class FloatGroup
{
    private Dictionary<object, float> group = new Dictionary<object, float>();

    // Methods
    public FloatGroup(){}
    public FloatGroup(float default_value){
        group.Add(this, default_value);
    }
    public void Add(object key, float value = 1)
    {
        group[key] = value;
    }
    public void Set(object key, float value){
        group[key] = value;
    }

    /// <summary>
    /// Get the minimum of all values (default = 0)
    /// </summary>
    public float GetMinimum(){
        float min_value = 0;
        bool min_default = true;
        foreach (float value in group.Values){
            if (min_default || value < min_value){
                min_default = false;
                min_value = value;
            }
        }
        return min_value;
    }
    
    /// <summary>
    /// Get the maximum of all values (default = 0)
    /// </summary>
    public float GetMaximum(){
        float max_value = 0;
        bool min_default = true;
        foreach (float value in group.Values){
            if (min_default || value > max_value){
                min_default = false;
                max_value = value;
            }
        }
        return max_value;
    }

    /// <summary>
    /// Returns the product of all values (default = 1)
    /// </summary>
    public float GetProduct(){
        float product = 1;
        foreach (float value in group.Values){
            product *= value;
        }
        return product;
    }

    /// <summary>
    /// Returns the sum of all values
    /// </summary>
    public float GetSum(){
        float sum = 0;
        foreach (float value in group.Values){
            sum += value;
        }
        return sum;
    }
}
