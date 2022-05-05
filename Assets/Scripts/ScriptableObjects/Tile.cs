using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using System.Text;
using System;

public class Tile : ScriptableObject
{
    public GameObject prefab;
    [ReadOnly]
    public bool symmetric;
    [HideInInspector]
    public HexSide rotation;
    public Quaternion rotationQuaternion => Quaternion.Euler(-90f, 180 + 60f * rotation, 0);

    [HorizontalLine]
    public UtilityType provider;
    public UtilityType receiver;

    [HorizontalLine]
    [ValidateInput("ValidateTerrainEdges")]
    public TerrainType[] terrainEdges = new TerrainType[6];
    [ValidateInput("ValidateUtilityEdges")]
    public UtilityType[] utilityEdges = new UtilityType[6];

    public (TerrainType, UtilityType) GetSide(HexSide side) => (terrainEdges[(int)side], utilityEdges[(int)side]);

    public Tile CopyAndRotate(HexSide side)
    {
        if (symmetric)
            throw new InvalidOperationException("Cannot rotate a symmetric tile");

        Tile copy = Instantiate(this);
        copy.name = name + " r" + ((int)side).ToString();
        copy.rotation = side;

        HexSide offset = side - rotation;
        copy.terrainEdges = new TerrainType[6];
        copy.utilityEdges = new UtilityType[6];
        
        for (int i=0;i<6;i++)
        {
            HexSide j = (i + offset);
            copy.terrainEdges[j] = terrainEdges[i];
            copy.utilityEdges[j] = utilityEdges[i];
        }

        return copy;
    }

    public void OnEnable()
    {
        //make sure prefab is centered
        prefab.transform.position = Vector3.zero;
    }

    public void OnValidate()
    {
        if (terrainEdges != null && utilityEdges != null && terrainEdges.Length == 6 && utilityEdges.Length == 6)
        {
            symmetric = true;
            var t = terrainEdges[0];
            var u = utilityEdges[0];
            for (int i=1;i<6;i++)
            {
                if (terrainEdges[i] != terrainEdges[i - 1] || utilityEdges[i] != utilityEdges[i - 1])
                {
                    symmetric = false;
                    break;
                }
            }
        }
        else symmetric = false;
    }

    private bool ValidateTerrainEdges(TerrainType[] arr) => arr.Length == 6;
    
    private bool ValidateUtilityEdges(UtilityType[] arr) => arr.Length == 6;
} 
