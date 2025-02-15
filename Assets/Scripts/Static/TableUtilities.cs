using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TableUtils
{
    // Utilities for tables

    /// <summary>
    /// Fill array with given value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="value">Value to fill the array with</param>
    public static void Fill<T>(this T[] array, T value){
        for (int i=0; i<array.Length; i++)
            array[i] = value;
    }

    /// <summary>
    /// Create an array filled with value of given length
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length">Length of the array</param>
    /// <param name="value">Value to fill the array with</param>
    /// <returns></returns>
    public static T[] ArrayFilled<T>(int length, T value){
        T[] array = new T[length];
        for (int i=0; i<array.Length; i++)
            array[i] = value;
        return array;
    }

    /// <summary>
    /// Add element to list only if it isn't contained yet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="elem">Element to add</param>
    public static void AddUnique<T>(this List<T> list, T elem){ // <T> is like Caml 'a polymorphic type
        // Add to list only if it doesn't already contain it
        if (!list.Contains(elem)) list.Add(elem);
    }

    /// <summary>
    /// Are the arrays equals by values
    /// </summary>
    /// <param name="a">First array</param>
    /// <param name="b">Second array</param>
    /// <returns></returns>
    public static bool ValuesEqual(this bool[] a, bool[] b){
        for (int i=0; i<a.Length; i++){
            if (a[i] != b[i]) return false;
        }
        return true;
    }
}
