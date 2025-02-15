using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializedDictionary<K,V>
{
    public K[] keys;
    public V[] values;

    public Dictionary<K,V> Build_dictionary()
    {
        Dictionary<K,V> result = new Dictionary<K, V>();
        int len = keys.Length > values.Length ? keys.Length : values.Length;
        for (int i=0; i<len; i++){
            result[keys[i]] = values[i];
        }
        return result;
    }
}
