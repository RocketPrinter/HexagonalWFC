using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum TerrainType
{
    Null = 0,
    Grass = 1,
    Forest = 2,
    Sand = 3
}

public static class TerrainTypeExtensions
{
    public static readonly (string name, Color color)[] All =
    {
        default,
        (nameof(TerrainType.Grass),new(98, 172, 104)),
        (nameof(TerrainType.Forest),new(100, 135, 103)),
        (nameof(TerrainType.Sand),new(245, 222, 145)),
    };

    public const float treshold = 20;

    public static string GetName(this TerrainType type) => All[(int)type].name;

    public static Color GetColor(this TerrainType type) => All[(int)type].color;

    public static TerrainType ToTerrainType(this Color c) => 
        Enumerable.Range(1, All.Length - 1)
        .Select(i => (i, All[i], All[i].color.DistanceSquared(c)))
        .OrderBy(p => p.i)
        .Select(p => (TerrainType)p.i)
        .FirstOrDefault();

    public static TerrainType ToTerrainType(this string s) => 
        Enumerable.Range(1, All.Length - 1)
        .Where(i => All[i].name == s)
        .Select(i => (TerrainType)i)
        .FirstOrDefault();
}

public enum UtilityType
{
    Null = 0,
    Road = 1,
    Rail = 2,
    Water = 3
}

public static class UtilityTypeExtensions
{
    public static readonly (string name, Color color)[] All =
    {
        default,
        (nameof(UtilityType.Road),new(73, 88, 103)),
        (nameof(UtilityType.Rail),new(189, 213, 234)),
        (nameof(UtilityType.Water),new(97, 132, 216)),
    };

    public const float treshold = 20;

    public static string GetName(this UtilityType type) => All[(int)type].name;

    public static Color GetColor(this UtilityType type) => All[(int)type].color;

    public static UtilityType ToUtilityType(this Color c) =>
        Enumerable.Range(1, All.Length - 1)
        .Select(i => (i, All[i], All[i].color.DistanceSquared(c)))
        .OrderBy(p => p.i)
        .Select(p => (UtilityType)p.i)
        .FirstOrDefault();

    public static UtilityType ToUtilityType(this string s) =>
        Enumerable.Range(1, All.Length - 1)
        .Where(i => All[i].name == s)
        .Select(i => (UtilityType)i)
        .FirstOrDefault();
}