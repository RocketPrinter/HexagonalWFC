using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using System.Linq;

public class GridManager : MonoBehaviour
{
    #region Inspector
    // to be used when slot has multiple superpositions
    [Header("Resources")]
    [Required]
    public GameObject superpositionPrefab;
    [Required]
    public TileSet tileset;

    [Header("Initial settings"), HorizontalLine]
    [ValidateInput("ValidateSize","size must be >0 and odd")]
    public int size;
    bool ValidateSize(int newSize) => newSize > 0 && newSize % 2 == 1;
    public int seed;

    [Header("Settings"), HorizontalLine]
    public bool throwOnInvalidStates = true;
    public bool instantUpdating = true;
    #endregion

    public Slot[,] grid;
    RandomQueue<(Slot slot, HexSide side, Slot other, RemoveSuperpositionsChange change)> updateQueue = new();
    Stack<IChange> changesStack = new();

    public HexPosition middle;

    void Awake()
    {
        grid = new Slot[size, size];
        middle = new HexPosition(size / 2, size / 2);

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                var pos = new HexPosition(i, j);
                if (InBounds(pos))
                    Slot.Create(this, pos);
            }
    }

    public bool InBounds(HexPosition hexPos)
    {
        if (!(0 <= hexPos.X && hexPos.X < size && 0 <= hexPos.Y && hexPos.Y < size))
            return false;
        return middle.Distance(hexPos) <= size/2;
    }

    #region Updates
    public void RegisterUpdate(Slot slot, HexSide side, Slot other, RemoveSuperpositionsChange change)
    {
        updateQueue.Push((slot, side, other, change));

        if (instantUpdating)
            UpdateAll();
    }

    [Button]
    public void UpdateOnce()
    {
        if (updateQueue.Count == 0)
            return;

        var info = updateQueue.PopRandom();
        info.slot.UpdateFromSide(info.side, info.other, info.change);
    }
    
    [Button]
    public void UpdateAll()
    {
        while (updateQueue.Count > 0)
            UpdateOnce();
    }
    
    public void AssertNoPendingUpdates()
    {
        if (updateQueue.Count > 0 && throwOnInvalidStates)
            throw new InvalidOperationException();
    }
    #endregion

    #region Operations
    public void RegisterChange(IChange op) => changesStack.Push(op);

    [Button("Undo")]
    public void UndoChange()
    {
        AssertNoPendingUpdates();
        if (changesStack.Count > 0)
            changesStack.Pop().Undo();
    }

    [Button]
    public void RefreshAll()
    {
        AssertNoPendingUpdates();

        throw new NotImplementedException();
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        if (updateQueue == null) return;
        var edgeOffsets = HexPosition.GetEdgeCenterOffsets().ToList();

        foreach (var tuple in updateQueue.Values)
        {
            Gizmos.color = new Color(0,1,0,0.1f);
            Gizmos.DrawSphere(tuple.slot.transform.position, 1.2f);
            Gizmos.color = Color.green;

            Vector3 from = tuple.other.transform.position + new Vector3(0,2,0);
            Vector3 to = tuple.slot.transform.position + new Vector3(0, 2, 0);

            Gizmos.DrawLine(from, to);

            float t = Time.timeSinceLevelLoad % 1;
            Vector3 spherePos = new Vector3(
                Mathf.SmoothStep(from.x,to.x,t),
                Mathf.SmoothStep(from.y,to.y,t),
                Mathf.SmoothStep(from.z,to.z,t));
            Gizmos.DrawSphere(spherePos, 0.1f);
        }
    }
    #endregion
}