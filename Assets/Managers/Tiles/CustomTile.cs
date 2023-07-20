using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New CustomTile", menuName = "ScriptableObject/Tile")]
public class CustomTile : ScriptableObject
{
    public TileBase tile;
    
    //the id MUST BE THE SAME as the ScriptableObject file name
    //otherwise the "Find" will fail
    [Header("MUST match the S.O. name")]
    public string id;

    public enum TileType
    {
        enemy,
        collectable,
        spike,
        rule
    }

    public TileType tileType;
    public int posX, posY;
    
    public void CheckNeighbours()
    {
        Debug.Log("posX = " + posX + ", posY = " + posY + ", tileType = " + tileType);
    }

    public void AssignPosition(int x, int y)
    {
        posX = x;
        posY = y;
        
    }
}
