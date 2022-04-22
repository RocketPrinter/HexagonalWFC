using UnityEngine;
using NaughtyAttributes;
using System.Collections;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;

#endif

[CreateAssetMenu(fileName = "TileSet", menuName = "TileSet", order = 0)]
public class TileSet : ScriptableObject
{
    [ReadOnly]
    public List<TileInfo> tiles;

#if UNITY_EDITOR
    public string path = "Assets/Tiles";

    [Button]
    void ProcessSelected()
    {
        
    }

    [Button]
    void ProcessAll()
    {
        
    }

    void ProcessPrefabs(List<GameObject> prefabs)
    {
        int progId = Progress.Start("Processing tiles...");

        EditorCoroutineUtility.StartCoroutine(Coroutine(), this);

        Progress.Remove(progId);

        IEnumerator Coroutine()
        {
            for (int i=0;i<prefabs.Count;i++)
            {
                GameObject prefab = prefabs[i];

                var pkg = tiles.FirstOrDefault(x => x.name == name);
                if (pkg == null)
                {
                    pkg = new()
                    {
                        name = prefab.name,
                        prefab = prefab
                    };
                    tiles.Add(pkg);
                }
                
                if (tiles.Select( t => t.name == prefab.name) )
                prefab.name

                Progress.Report(progId, i, prefabs.Count);
                yield return null;
            }

            yield break;
        }

        void Process(TileInfo pkg)
        {
            
        }
    }
#endif
}
