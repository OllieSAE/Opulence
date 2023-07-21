using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : iHeapItem<Node>
{
    public bool isTile;
    public bool fullNeighbours;
    public Node parent;
    public Node[,] neighbours = new Node[3, 3];

    public Vector3 GridPosVector3
    {
        get
        {
            return new Vector3(gridPosition.x, gridPosition.y, 0);
        }
    }

    public Vector3 gridPositionGizmosOnly
    {
        get
        {
            return new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);
        }
    }

    public Vector3Int gridPosV3Int
    {
        get
        {
            return new Vector3Int((int) gridPosition.x, (int) gridPosition.y, 0);
        }
    }
    
    public Vector3 gridPosition;
    public Vector3 worldPosition;
    public int xPosInArray;
    public int yPosInArray;
    
    private int _heapIndex;

    public int gCost;
    public int hCost;

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    
    public int CompareTo(Node nodeToCompare) //returns 1 if nodeToCompare is HIGHER
    {
        //check this node's fCost against node to compare
        int compare = fCost.CompareTo(nodeToCompare.fCost);
            
        //0 if identical f cost, so check h cost
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
            
        //we want to return 1 if higher priority, but this returns 1 if nodeToCompare is higher so need to reverse
        return -compare;
    }

    public int heapIndex
    {
        get
        {
            return _heapIndex;
        }
        set
        {
            _heapIndex = value;
        }
    }
}
