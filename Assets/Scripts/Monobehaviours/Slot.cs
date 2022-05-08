using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase] // very cool attribute
public class Slot : MonoBehaviour
{
    public class InvalidSuperpositionState : Exception { }

    public GridManager manager;
    public HexPosition hexPos;

    public HashSet<Tile> superpositions;
    public int entropy => superpositions.Count;
    public bool isCollapsed => superpositions.Count == 1;
    public bool isInvalid => superpositions.Count == 0;

    // used for visuals
    bool queueVisualsUpdate;
    GameObject child;
    TextMeshPro childTextCache;

    // caching
    bool sideCacheValid = false;
    public HashSet<(TerrainType, UtilityType)>[] _cache = new HashSet<(TerrainType, UtilityType)>[6];

    int lastCachedEntropy = -1;

    #region init
    public static Slot Create(GridManager manager, HexPosition hexPos)
    {
        if (!manager.InBounds(hexPos) || manager.grid[hexPos.X, hexPos.Y] != null)
            throw new InvalidOperationException();

        var go = new GameObject($"Slot {{ {hexPos.X}, {hexPos.Y}}}");
        go.transform.SetParent(manager.transform);
        var v2 = hexPos.ToGrid() - manager.middle.ToGrid();
        go.transform.position = new Vector3(v2.x,0,v2.y);
        
        var slot = go.AddComponent<Slot>();
        slot.manager = manager;
        slot.hexPos = hexPos;
        manager.grid[hexPos.X, hexPos.Y] = slot;
        slot.superpositions = manager.tileset.GetTiles().ToHashSet();

        slot.UpdateCaches();
        return slot;
    }
    #endregion

    #region Caches
    public HashSet<(TerrainType, UtilityType)> GetSideCache(HexSide side)
    {
        if (sideCacheValid == false)
        {
            sideCacheValid = true;
            for (int i=0;i<6;i++)
            {
                _cache[i] = superpositions.Select(x => x.GetSide(i)).ToHashSet();
            }
        }

        return _cache[side];
    }

    void UpdateCaches()
    {
        sideCacheValid = false;
        UpdateEntropyCache();
        queueVisualsUpdate = true;
    }

    void UpdateEntropyCache()
    {
        if (entropy == lastCachedEntropy)
            return;
        manager.UpdateEntropyCache(this, lastCachedEntropy, entropy);
        lastCachedEntropy = entropy;
    }
    #endregion

    #region Superpositions
    public void AddSuperpositionWithoutPropagation(Tile tile)
    {
        superpositions.Add(tile);

        UpdateCaches();
    }

    // collapses tile and propagates changes
    public void Collapse(Tile tile)
    {
        if (isCollapsed || isInvalid) return;
        manager.AssertNoPendingUpdates();

        Debug.Assert(superpositions.Contains(tile));
        manager.RegisterRemovedSuperpositions(this, superpositions.Where(t => t != tile));
        
        superpositions.Clear();
        superpositions.Add(tile);

        UpdateCaches();
        
        UpdateNeighbours();
    }

    // collapses tile and propagates changes
    public void CollapseRandom()
    {
        if (isCollapsed || isInvalid) return;
        manager.AssertNoPendingUpdates();

        // pick random tile using bias
        Tile target = superpositions.Select(t => (manager.rand.NextDouble() / t.bias, t))
            .Aggregate((bestP, p) => p.Item1 <= bestP.Item1 ? p : bestP).Item2;

        Collapse(target);
    }

    // NEVER CALL DIRECTLY! Use manager.RegisterUpdate
    public void UpdateFromSide(HexSide side, Slot otherSlot)
    {
        if (isCollapsed) return;

        var otherSide = otherSlot.GetSideCache(side.Opposite);
        var remove = GetSideCache(side)
            .Where(p => !otherSide.Contains(p)) // find pairs that can't connect
            .Select(p => superpositions.Where(x=>x.GetSide(side) == p) ) // find tiles with these pairs
            .SelectMany(x=>x)
            .ToList();

        if (remove.Count == 0) return;

        manager.RegisterRemovedSuperpositions(this, remove);
        foreach (var tile in remove)
            superpositions.Remove(tile);
        UpdateCaches();

        UpdateNeighbours(side);
    }

    // propagates to neighbouring slots
    void UpdateNeighbours()
    {
        int i = 0;
        foreach (var pos in hexPos.GetNeighbours())
        {
            if (manager.InBounds(pos))
                manager.RegisterUpdate(new(manager.grid[pos.X, pos.Y], new HexSide(i).Opposite, this));
            i++;
        }
    }

    // propagates to neighbouring slots minus one
    void UpdateNeighbours(HexSide minus)
    {
        int i = 0;
        foreach (var pos in hexPos.GetNeighbours())
        {
            if (manager.InBounds(pos) && i != minus)
                manager.RegisterUpdate(new(manager.grid[pos.X, pos.Y], new HexSide(i).Opposite, this));
            i++;
        }
    }
    #endregion

    #region Visuals
    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        // update visuals
        if (queueVisualsUpdate)
        {
            queueVisualsUpdate = false;
            UpdateVisuals();
        }
    }

    // todo: optimise gameobject use and maybe use mesh instancing or or something I dunno
    void UpdateVisuals()
    {
        if (isCollapsed)
        {
            var tile = superpositions.First();
            // Unity doesn't recognise the difference between instanciated GOs and prefabs outside editor so this will have to do
            if (child != null && child.name.StartsWith(tile.name))
                return;

            if (child != null)
                Destroy(child);
            child = Instantiate(tile.prefab, transform.position, tile.rotationQuaternion, transform);
            child.name = tile.name; // makes sure prefab matches tile name

            return;
        }

        if (child == null || !child.name.StartsWith(manager.superpositionPrefab.name))
        {
            if (child != null)
                Destroy(child);
            child = Instantiate(manager.superpositionPrefab, transform);
            childTextCache = child.GetComponentInChildren<TextMeshPro>();
        }

        childTextCache.text = entropy.ToString();
        childTextCache.color = entropy > 0 ? Color.white : Color.red;
    }
    #endregion

    #region Debug
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false || !manager.visualizeSlotCaches) return;
        var edgeCenters = HexPosition.GetEdgeCenterOffsets().Select(v2 => transform.position + new Vector3(v2.x, 2, v2.y) * 0.9f).ToList();

        for (int i=0;i<6;i++)
        {
            int j = 0;
            foreach (var pair in GetSideCache(i))
            {
                Gizmos.color = pair.Item1.GetColor();
                Gizmos.DrawSphere(edgeCenters[i] + new Vector3(0, j * 0.3f, 0), 0.1f);

                j++;
            }
        }
    }

    [Dropdown("GetTileDropdown")]
    public Tile tileSelector = null;

    DropdownList<Tile> GetTileDropdown()
    {
        var list = new DropdownList<Tile>();
        if (isInvalid)
            list.Add("NONE",null);
        else foreach (var tile in superpositions)
            {
                list.Add(tile.name, tile);
            }
        return list;
    }

    [Button("Collapse")]
    void CollapseButton()
    {
        if (tileSelector == null || !superpositions.Contains(tileSelector)) return;

        Collapse(tileSelector);
    }

    [Button]
    void GoToManager() => Selection.activeObject = manager;
#endif
    #endregion
}