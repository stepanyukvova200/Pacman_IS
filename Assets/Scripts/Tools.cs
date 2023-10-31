using System;
using UnityEngine;

public static class Tools
{
    private const float eps = 1e-2f;
    public static bool EqualVector(this Vector3 lhs, Vector3 rhs)
    {
        return (lhs - rhs).sqrMagnitude <= eps * eps;
    }
    public static bool EqualVector(this Vector2 lhs, Vector2 rhs)
    {
        return (lhs - rhs).sqrMagnitude <= eps * eps;
    }

    public static int IndexOf<T>(this T[] array, T el)
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (el.Equals(array[i]))
                return i;
        }

        return -1;
    }
}