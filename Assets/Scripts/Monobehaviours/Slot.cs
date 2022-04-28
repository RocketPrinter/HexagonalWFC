using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Slot : MonoBehaviour
{
    public GridManager manager;
    public HexPosition hexPos;

    HashSet<Tile> superpositions = new();
    
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
