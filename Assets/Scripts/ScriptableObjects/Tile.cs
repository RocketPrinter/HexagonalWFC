using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using System.Text;

public class Tile : ScriptableObject
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

    //public Connection GetConnection()
    //{
    //    
    //}

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

    [Button]
    void ShowTerrains() => Debug.Log(new StringBuilder().AppendJoin(" ", terrains.Select(x => x.name)));
    
    [Button]
    void ShowUtilities() => Debug.Log(new StringBuilder().AppendJoin(" ", utilities.Select(x => x.name)));
}
