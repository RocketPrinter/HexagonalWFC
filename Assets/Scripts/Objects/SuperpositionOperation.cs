using System;
using System.Collections.Generic;
using System.Linq;

public class SuperpositionOperation : IOperation
{
    public Slot slot { get; init; }
    public Tile tile { get; init; }

    List<(Slot slot, Tile removed)> changes=new();
    
    public SuperpositionOperation(Slot slot, Tile tile)
    {
        this.slot = slot;
        this.tile = tile;
    }

    // collapses target slot to tile
    public void Execute()
    {
        try
        {
            slot.Collapse(tile, this);
        }
        catch (Slot.InvalidSuperpositionState)
        {
            throw new NotImplementedException();
        }
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