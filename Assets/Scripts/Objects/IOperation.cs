using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOperation
{
    void Execute();
    void Undo();
}

public class MarkerOperation : IOperation
{
    public void Execute()
    {
        
    }

    public void Undo()
    {
        
    }
}