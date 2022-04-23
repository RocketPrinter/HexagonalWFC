using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TileSO : ScriptableObject
{
    public GameObject prefab;

    [HorizontalLine]
    [Dropdown(nameof(UtilityDropdown))]
    public UtilityType provider;
    [Dropdown(nameof(UtilityDropdown))]
    public UtilityType receiver;

    [HorizontalLine]
    public TerrainType[] terrains;
    public UtilityType[] utilities;

    DropdownList<TerrainType> TerrainDropdown()
    {
        DropdownList<TerrainType> list = new();
        foreach (var item in TerrainType.All)
        {
            list.Add(item.name, item);
        }
        return list;
    }
    
    DropdownList<UtilityType> UtilityDropdown()
    {
        DropdownList<UtilityType> list = new();
        foreach (var item in UtilityType.All)
        {
            list.Add(item.name, item);
        }
        return list;
    }
}
