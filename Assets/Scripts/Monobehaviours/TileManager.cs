using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TileManager : MonoBehaviour
{
    [Required]
    public TileSet tileset;

    Stack<IOperation> opStack;

    public void ExecuteOperation(IOperation op)
    {
        opStack.Push(op);
        op.Execute();
    }

    public void UndoOperation()
    {
        if (opStack.Count > 0)
            opStack.Pop().Undo();
    }

    public void UndoUntilMarker()
    {
        while (opStack.Count > 0)
        {
            var op = opStack.Pop();
            op.Undo();
            if (op is MarkerOperation) break;
        }
    }
}