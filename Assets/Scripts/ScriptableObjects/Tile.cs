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
    public TerrainType[] terrainEdges = new TerrainType[6];
    public UtilityType[] utilityEdges = new UtilityType[6];

    public (TerrainType, UtilityType) GetSide(HexSide side) => (terrainEdges[(int)side], utilityEdges[(int)side]);
    
    public void OnEnable()
    {
        //make sure prefab is centered
        prefab.transform.position = Vector3.zero;
    }
} 
