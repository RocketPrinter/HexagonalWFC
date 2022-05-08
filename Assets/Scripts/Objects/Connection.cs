using System.Collections.Generic;
using System;

public struct Connection
{
    public Slot slot;
    public Tile removed;

    public Connection(Slot slot, Tile removed)
    {
        this.slot = slot;
        this.removed = removed;
    }

    public override bool Equals(object obj)
    {
        return obj is Connection other &&
               EqualityComparer<Slot>.Default.Equals(slot, other.slot) &&
               EqualityComparer<Tile>.Default.Equals(removed, other.removed);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(slot, removed);
    }

    public void Deconstruct(out Slot slot, out Tile removed)
    {
        slot = this.slot;
        removed = this.removed;
    }

    public static implicit operator (Slot slot, Tile removed)(Connection value)
    {
        return (value.slot, value.removed);
    }

    public static implicit operator Connection((Slot slot, Tile removed) value)
    {
        return new Connection(value.slot, value.removed);
    }
}