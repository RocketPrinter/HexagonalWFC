using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static float DistanceSquared(this Color color, Color otherColor)
    {
        float dr = color.r - otherColor.r;
        float dg = color.g - otherColor.g;
        float db = color.b - otherColor.b;

        return dr * dr + dg * dg + db * db;
    }
}