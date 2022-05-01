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
    public UtilityType provider;
    public UtilityType receiver;

    [HorizontalLine]
    public TerrainType[] terrains;
    public UtilityType[] utilities;

    //public Connection GetConnection()
    //{
    //    
    //}
} 
