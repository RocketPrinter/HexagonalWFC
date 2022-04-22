using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainType
{
    public static readonly TerrainType[] All = { Grass, Forest, Sand, Water };
    
    public static readonly TerrainType Grass = new(nameof(Grass),new(98, 172, 104));
    public static readonly TerrainType Forest = new(nameof(Forest), new(100, 135, 103));
    public static readonly TerrainType Sand = new(nameof(Sand), new(245, 222, 145));
    public static readonly TerrainType Water = new(nameof(Water), new(97, 132, 216));

    public readonly string name;
    public readonly Color color;

    public static implicit operator Color(TerrainType type) => type.color;

    public static TerrainType FromColor(Color c) => All
        .Select(t => (t,t.color.DistanceSquared(c)))
        .OrderBy(x => x.Item2)
        .Select(x=> x.t)
        .First();

    public static TerrainType FromString(string s) => All
        .Where(t => t.name == s)
        .First();

    private TerrainType(string name, Color color)
    {
        this.name = name;
        this.color = color;
    }
}

public class UtilityType
{
    public static UtilityType[] All = { Road, Rail, Water };

    public static UtilityType Road = new(nameof(Road), new(73, 88, 103));
    public static UtilityType Rail = new(nameof(Rail), new(189, 213, 234));
    public static UtilityType Water = new(nameof(Water), new(97, 132, 216));

    public readonly string name;
    public readonly Color color;

    public static implicit operator Color(UtilityType type) => type.color;

    public static UtilityType FromColor(Color c) => All
        .Select(t => (t, t.color.DistanceSquared(c)))
        .OrderBy(x => x.Item2)
        .Select(x => x.t)
        .First();

    public static UtilityType FromString(string s) => All
        .Where(t => t.name == s)
        .First();

    private UtilityType(string name, Color color)
    {
        this.name = name;
        this.color = color;
    }
}