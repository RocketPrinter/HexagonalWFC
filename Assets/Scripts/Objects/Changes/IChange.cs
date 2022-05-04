using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChange
{
    void Undo();
}

public class CompositeChange : IChange
{
    IChange[] changes;
    
    public CompositeChange(params IChange[] changes)
    {
        this.changes = changes;
    }

    public void Undo()
    {
        foreach (var change in changes)
            change.Undo();
    }
}