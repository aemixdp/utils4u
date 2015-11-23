using UnityEngine;
using System;
using System.Collections.Generic;

public static class Extensions
{
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> knownKeys = new HashSet<TKey>();
        foreach (TSource element in source)
        {
            if (knownKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static Vector2 Rotate(this Vector2 vec, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        float tx = vec.x;
        float ty = vec.y;
        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }

    public static GameObject ChildWithTag(this GameObject parent, string tag)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.tag == tag)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public static T RandomElement<T>(this T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
}
