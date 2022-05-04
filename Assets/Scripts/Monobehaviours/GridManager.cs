using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class GridManager : MonoBehaviour
{
    // to be used when slot has multiple superpositions
    [Required]
    public GameObject superpositionPrefab;
    [Required]
    public TileSet tileset;
    
    [HorizontalLine, ValidateInput("ValidateSize","size must be >0 and odd")]
    public int size;
    bool ValidateSize(int newSize) => newSize > 0 && newSize % 2 == 1;

    public bool blockPropagations; // todo: implement dummy
    public bool randomizeNeighbourPropagationOrder; // todo: implement dummy
    public bool throwOnInvalidStates;

    public Slot[,] grid;
    Stack<IOperation> opStack = new();

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

    #region Operations
    public void ExecuteOperation(IOperation op)
    {
        opStack.Push(op);
        op.Execute();
    }

    [Button]
    public void RefreshAll()
    {
        throw new NotImplementedException();
    }

    [Button("Undo")]
    public void UndoOperation()
    {
        if (opStack.Count > 0)
            opStack.Pop().Undo();
    }
    #endregion
}