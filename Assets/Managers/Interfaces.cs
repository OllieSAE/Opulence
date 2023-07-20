using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IHealth
{
    public void TakeDamage(int amount);
}

public interface IDamageable
{
    
}

public interface iHeapItem<T> : IComparable<T>
{
    int heapIndex
    {
        get;
        set;
    }
}

public interface iPathable
{
    void GeneratePath(Node node);
    void ClearPath();
}

