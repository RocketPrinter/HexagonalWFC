using System;
using System.Collections.Generic;
using System.Linq;

public class RemoveSuperpositionsChange : IChange
{
    List<(Slot slot, Tile removed)> changes=new();
    
    public RemoveSuperpositionsChange()
    {
        
    }

    public void Undo()
    {
        foreach (var pair in changes)
        {
            pair.slot.AddSuperpositionWithoutPropagation(pair.removed);
        }
    }

    public void RegisterRemoved(Slot slot, Tile removed) => changes.Add((slot,removed));
    public void RegisterRemoved(Slot slot, IEnumerable<Tile> removed) => changes.AddRange(removed.Select(r=> (slot, r)));
}