using System.Collections;
using System.Collections.Generic;
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
}
