
using System;

/// <summary>
/// Simple class with a min and a max (int), Serializable
/// </summary>
[Serializable]
public class MinMaxInt
{
    public MinMaxInt(int Min, int Max){
        min = Min;
        max = Max;
    }
    public int min;
    public int max;
    public override string ToString() => $"({min}, {max})";

    /// <summary>
    /// Inclusive int number between min and max
    /// </summary>
    public int GetRandom(){
        return UnityEngine.Random.Range(min, max+1);
    }
}
[Serializable]

/// <summary>
/// Simple class with a min and a max (float), Serializable
/// </summary>
public class MinMaxFloat
{
    public MinMaxFloat(float Min, float Max){
        min = Min;
        max = Max;
    }
    public float min;
    public float max;
    public override string ToString() => $"({min}, {max})";

    /// <summary>
    /// Random float between min and max
    /// </summary>
    public float GetRandom(){
        return UnityEngine.Random.Range(min, max);
    }
}
