using System;
using UnityEngine;

public static class ConvertExtensions
{
    public static int ToInt(this bool[] values)
    {
        int result = 0;
        int depth = values.Length;
        for (int i = depth - 1; i >= 0; i--)
        {
            if (values[i])
            {
                result += (int)Mathf.Pow(2, depth - 1 - i);
            }
        }
        return result;
    }

    public static string ToStringBool(this bool[] values)
    {
        string result = "";
        for (int i = 0; i < values.Length; i++)
        {
            result += $"{(values[i] ? 1 : 0)}";
        }
        return result;
    }

    public static string ToStringInt(this int[] values)
    {
        string result = "";
        for (int i = 0; i < values.Length; i++)
        {
            result += $"{values[i]}";
        }
        return result;
    }

    public static bool[] ToBinaryBool(this int intValue, int bitDepth)
    {
        bool[] valueArray = new bool[bitDepth];
        int value = intValue;
        for (int i = bitDepth - 1; i >= 0; i--)
        {
            valueArray[i] = value % 2 == 1;
            value = value / 2;
        }
        return valueArray;
    }

    public static int[] ToBinaryInt(this int intValue, int bitDepth)
    {
        int[] valueArray = new int[bitDepth];
        int value = intValue;
        for (int i = bitDepth - 1; i >= 0; i--)
        {
            valueArray[i] = value % 2;
            value = value / 2;
        }
        return valueArray;
    }
}
