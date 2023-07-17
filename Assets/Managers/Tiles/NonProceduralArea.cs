using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NonProceduralArea", menuName = "ScriptableObject/NonProceduralArea")]
public class NonProceduralArea : ScriptableObject
{
    public GameObject prefab;
    public Vector2 spawnPosition;
    [Header("Use EVEN numbers")]
    public int height;
    public int width;
    
}
