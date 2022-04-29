using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Slot : MonoBehaviour
{
    public class InvalidSuperpositionState : Exception { }

    public GridManager manager;
    public HexPosition hexPos;

    HashSet<Tile> superpositions = new();
    public IReadOnlyCollection<Tile> Superpositions => superpositions;

    GameObject child;

    #region Creation
    public static Slot Create(GridManager manager, HexPosition hexPos)
    {
        if (!manager.InBounds(hexPos) || manager.grid[hexPos.X, hexPos.Y] != null)
            throw new InvalidOperationException();

        var go = new GameObject($"Slot {{ {hexPos.X}, {hexPos.Y}}}");
        var v2 = hexPos.ToGrid();
        go.transform.position = new Vector3(v2.x,0,v2.y);
        
        var slot = go.AddComponent<Slot>();
        slot.manager = manager;
        slot.hexPos = hexPos;
        manager.grid[hexPos.X, hexPos.Y] = slot;

        return slot;
    }

    private void Start()
    {
        UpdateSelf();
    }
    #endregion

    #region superpositions
    public void AddSuperpositionWithoutPropagation(Tile tile)
    {
        superpositions.Add(tile);
        UpdateSelf();
    }

    public void RemoveSuperposition(Tile tile, SuperpositionOperation op)
    {
        superpositions.Remove(tile);
        op.RegisterRemoved(this,tile);
        UpdateNeighbours(op);
        UpdateSelf();
    }

    public void Collapse(Tile tile, SuperpositionOperation op)
    {
        Debug.Assert(superpositions.Contains(tile));
        op.RegisterRemoved(this, superpositions.Where(t=>t!=tile));
        
        superpositions.Clear();
        superpositions.Add(tile);
        
        UpdateNeighbours(op);
        UpdateSelf();
    }

    public void UpdateSuperpositions(SuperpositionOperation op)
    {
        for (int i = 0; i < 5; i++)
            UpdateSuperpositions(i, op);
    }

    public void UpdateSuperpositions(HexSide side, SuperpositionOperation op)
    {
        throw new NotImplementedException();
    }

    void UpdateNeighbours(SuperpositionOperation op)
    {
        int i = 0;
        foreach (var pos in hexPos.GetNeighbours())
        {
            manager.grid[pos.X, pos.Y].UpdateSuperpositions( new HexSide(i).Opposite, op);
            i++;
        }
    }
    void UpdateSelf()
    {
        if (superpositions.Count == 1)
        {
            var prefab = superpositions.First().prefab;
            // Unity doesn't recognise the difference between instanciated GOs and prefabs outside editor so this will have to do
            if (child != null && child.name.StartsWith(prefab.name))
                return;

            if (child != null)
                Destroy(child);
            child = Instantiate(prefab, transform);

            return;
        }

        if (child == null || !child.name.StartsWith(manager.superpositionPrefab.name))
        {
            if (child != null)
                Destroy(child);
            child = Instantiate(manager.superpositionPrefab, transform);
        }

        throw new NotImplementedException(); // todo: lazy
    }
    #endregion

    private void OnDrawGizmos()
    {
        const float mul = 0.9f;
        var coords = hexPos.GetVertexOffsets().Select(v2 => transform.position + new Vector3(v2.x,0,v2.y) * mul).ToList();

        Gizmos.color = hexPos == manager.middle ? Color.yellow : Color.green;
        
        for (int i = 1; i < coords.Count; i++)
            Gizmos.DrawLine(coords[i-1],coords[i]);
        Gizmos.DrawLine(coords[coords.Count-1],coords[0]);
    }
}