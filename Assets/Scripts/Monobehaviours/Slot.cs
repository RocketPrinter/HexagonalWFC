using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using NaughtyAttributes;

[SelectionBase] // very cool attribute
public class Slot : MonoBehaviour
{
    public class InvalidSuperpositionState : Exception { }

    public GridManager manager;
    public HexPosition hexPos;

    HashSet<Tile> superpositions;
    public IReadOnlyCollection<Tile> Superpositions => superpositions;
    public bool collapsed => superpositions.Count == 1;

    // used for visuals
    bool queueVisualsUpdate;
    GameObject child;
    TextMeshPro childTextCache;

    // caching
    bool sideCacheValid = false;
    public HashSet<(TerrainType, UtilityType)>[] _cache = new HashSet<(TerrainType, UtilityType)>[6];

    #region Creation and loops
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

        return slot;
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void Update()
    {
        // update visuals
        if (queueVisualsUpdate)
        {
            queueVisualsUpdate = false;
            UpdateVisuals();
        }
    }
    #endregion

    #region side cache
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
    #endregion

    #region superpositions
    public void AddSuperpositionWithoutPropagation(Tile tile)
    {
        superpositions.Add(tile);
        sideCacheValid = false;
        queueVisualsUpdate = true;
    }

    // collapses tile and propagates changes
    public void Collapse(Tile tile, CollapseOperation op)
    {
        Debug.Assert(superpositions.Contains(tile));
        op.RegisterRemoved(this, superpositions.Where(t=>t!=tile));
        
        superpositions.Clear();
        superpositions.Add(tile);

        sideCacheValid = false;
        queueVisualsUpdate = true;
        
        PropagateToNeighbours(op);
    }

    void UpdateSuperpositionsFromSide(HexSide side, Slot otherSlot, CollapseOperation op)
    {
        if (collapsed) return;

        var otherSide = otherSlot.GetSideCache(side.Opposite);
        var remove = GetSideCache(side)
            .Where(p => !otherSide.Contains(p)) // find in cache pairs that can't connect
            .Select(p => superpositions.Where(x=>x.GetSide(side) == p) ) // find tiles with these pairs
            .SelectMany(x=>x)
            .ToList();

        if (remove.Count == 0) return;

        op.RegisterRemoved(this, remove);
        foreach (var tile in remove)
            superpositions.Remove(tile);
        sideCacheValid = false;
        queueVisualsUpdate = true;

        PropagateToNeighbours(side, op);
    }

    // propagates to neighbouring slots
    void PropagateToNeighbours(CollapseOperation op)
    {
        int i = 0;
        foreach (var pos in hexPos.GetNeighbours())
        {
            if (manager.InBounds(pos))
                manager.grid[pos.X, pos.Y].UpdateSuperpositionsFromSide( new HexSide(i).Opposite, this, op);
            i++;
        }
    }

    // propagates to neighbouring slots minus one
    void PropagateToNeighbours(HexSide minus, CollapseOperation op)
    {
        int i = 0;
        foreach (var pos in hexPos.GetNeighbours())
        {
            if (manager.InBounds(pos) && i != minus)
                manager.grid[pos.X, pos.Y].UpdateSuperpositionsFromSide(new HexSide(i).Opposite, this, op);
            i++;
        }
    }
    
    void UpdateVisuals()
    {
        if (collapsed)
        {
            var tile = superpositions.First();
            // Unity doesn't recognise the difference between instanciated GOs and prefabs outside editor so this will have to do
            if (child != null && child.name.StartsWith(tile.name))
                return;

            if (child != null)
                Destroy(child);
            child = Instantiate(tile.prefab, transform.position, tile.rotationQuaternion, transform);
            child.name = tile.name; // makes sure prefab maches tile name

            return;
        }

        if (child == null || !child.name.StartsWith(manager.superpositionPrefab.name))
        {
            if (child != null)
                Destroy(child);
            child = Instantiate(manager.superpositionPrefab, transform);
            childTextCache = child.GetComponentInChildren<TextMeshPro>();
        }

        childTextCache.text = superpositions.Count.ToString();
        childTextCache.color = superpositions.Count > 0 ? Color.white : Color.red;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        const float mul = 0.9f;
        var coords = hexPos.GetVertexOffsets().Select(v2 => transform.position + new Vector3(v2.x,0,v2.y) * mul).ToList();

        Gizmos.color = hexPos == manager.middle ? Color.yellow : Color.green;
        
        for (int i = 1; i < coords.Count; i++)
            Gizmos.DrawLine(coords[i-1],coords[i]);
        Gizmos.DrawLine(coords[coords.Count-1],coords[0]);
    }

    [Dropdown("GetTileDropdown")]
    public Tile tileSelector = null;

    DropdownList<Tile> GetTileDropdown()
    {
        var list = new DropdownList<Tile>();
        if (superpositions.Count == 0)
            list.Add("NONE",null);
        else foreach (var tile in superpositions)
            {
                list.Add(tile.name, tile);
            }
        return list;
    }

    [Button("Collapse")]
    public void CollapseButton()
    {
        if (tileSelector == null || !superpositions.Contains(tileSelector)) return;

        manager.ExecuteOperation(new CollapseOperation(this, tileSelector));
    }
#endif
}