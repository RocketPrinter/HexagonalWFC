using UnityEngine;
using NaughtyAttributes;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

[CreateAssetMenu(fileName = "TileSet", menuName = "TileSet", order = 0)]
public class TileSet : ScriptableObject
{
    [ReadOnly]
    public List<TileSO> tiles;

#if UNITY_EDITOR
    [InfoBox(".fbx, .blend etc file here")]
    public string path = "Assets/Tiles";

    readonly Ray[] rays = GenerateRays();

    static Ray[] GenerateRays()
    {
        var dir = new Vector3(0, -1, 0);
        float mul = 0.9f;
        float h = Mathf.Sqrt(3) / 2;
        h *= mul;

        return new Ray[6]
        {
            new(new(mul/2 , 1, h ),dir),
            new(new(mul   , 1, 0 ),dir),
            new(new(mul/2 , 1, -h),dir),
            new(new(-mul/2, 1, -h),dir),
            new(new(-mul  , 1, 0 ),dir),
            new(new(-mul/2, 1, h ),dir)
        };
    }

    /*[Button]
    void ProcessSelected()
    {
        var gos = Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered);

        ProcessPrefabs(gos.ToList());

        Debug.Log($"Processed {gos.Length} prefabs");
    }*/

    [Button]
    void ProcessAll()
    {   
        List<string> paths = new();
        RecursiveSearch(new DirectoryInfo(path + "/Meshes"));

        var prefabs = paths.Select(p => AssetDatabase.LoadAllAssetsAtPath(p))
            .SelectMany(a => a)
            .Select(x => x as GameObject)
            .Where(x => x != null)
            .ToList();

        ProcessPrefabs(prefabs);

        void RecursiveSearch(DirectoryInfo dir)
        {
            foreach (var fi in dir.GetFiles())
            {
                if (fi.Extension != "meta")
                    paths.Add(fi.FullName[fi.FullName.LastIndexOf("Assets")..]);
            }

            foreach (var di in dir.GetDirectories())
                RecursiveSearch(di);
        }
    }

    void ProcessPrefabs(List<GameObject> prefabs)
    {
        int progId = Progress.Start("Processing tiles...");

        EditorCoroutineUtility.StartCoroutine(Coroutine(), this);

        Progress.Remove(progId);

        Clean();

        IEnumerator Coroutine()
        {
            for (int i = 0; i < prefabs.Count; i++)
            {
                ProcessPrefab(prefabs[i]);
                
                Progress.Report(progId, i, prefabs.Count);
                yield return null;
            }

            yield break;
        }
    }

    void ProcessPrefab(GameObject prefab)
    {
        TileSO so = tiles.Find(x => x.prefab);
        if (so == null)
        {
            so = new()
            {
                name = prefab.name,
                prefab = prefab
            };
            AssetDatabase.CreateAsset(so, path + "/Tiles/" + prefab.name);
        }
        
        var colors = ColorPicker.ColorpickMultiple(prefab,rays);
    }

    void Clean()
    {
        var deleted = tiles.Where(x => x.prefab == null).ToList();
        tiles = tiles.Except(deleted).ToList();

        List<string> failedPaths = new();
        AssetDatabase.DeleteAssets(deleted.Select(x => AssetDatabase.GetAssetPath(x)).ToArray(), failedPaths);
        if (failedPaths.Count > 0)
            Debug.LogWarning($"Failed to deleted {failedPaths.Count} assets");
    }
    
    /*[Button]
    void Test()
    {
        var dic = Selection.objects.Select(x => x.GetType())
            .Aggregate(new Dictionary<Type, int>(), (acc, t) =>
            {
                if (!acc.TryAdd(t, 1))
                    acc[t]++;
                return acc;
            });

        foreach (var kvp in dic)
        {
            Debug.Log($"{kvp.Key.ToString()}     {kvp.Value}");
        }
    }

    [Button]
    void Test2()
    {
        List<string> paths = new();
        RecursiveSearch(new DirectoryInfo(path));

        foreach (var p in paths)
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(p))
            {
                Debug.Log(obj);
            }

        void RecursiveSearch(DirectoryInfo dir)
        {
            foreach (var fi in dir.GetFiles())
            {
                if (fi.Extension != "meta")
                    paths.Add(fi.FullName[fi.FullName.LastIndexOf("Assets")..]);
            }

            foreach (var di in dir.GetDirectories())
                RecursiveSearch(di);
        }
    }
    */
#endif
}
