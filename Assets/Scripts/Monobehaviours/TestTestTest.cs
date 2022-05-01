using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class TestTestTest : MonoBehaviour
{
    public GameObject prefab;

    public ColorPicker.ColorPickerResult results;
    public TerrainType[] terrains;
    public UtilityType[] utilities;

    [Button]
    public void Test()
    {
        var results = ColorPicker.ColorpickMultiple(prefab, GenerateRays());
        terrains = results.Select(x => x.color.ToTerrainType()).ToArray();
        utilities = results.Select(x => x.color.ToUtilityType()).ToArray();
    }

    static Ray[] GenerateRays()
    {
        var dir = new Vector3(0, -1, 0);
        float mul = 0.9f;
        float h = Mathf.Sqrt(3) / 2;
        h *= mul;

        return new Ray[12]
        {
            // terrains 
            new(new(mul/2 , 10, h ),dir),
            new(new(mul   , 10, 0 ),dir),
            new(new(mul/2 , 10, -h),dir),
            new(new(-mul/2, 10, -h),dir),
            new(new(-mul  , 10, 0 ),dir),
            new(new(-mul/2, 10, h ),dir),

            // utilities
            new(new(0     , 10, h   ),dir),
            new(new(0.75f , 10, h/2 ),dir),
            new(new(0.75f , 10, -h/2),dir),
            new(new(0     , 10, -h  ),dir),
            new(new(-0.75f, 10, -h/2),dir),
            new(new(-0.75f, 10, h/2 ),dir),
        };
    }
}
