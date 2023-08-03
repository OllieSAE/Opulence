using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueLocation : MonoBehaviour
{
    private LevelGenerator levelGenerator;
    public Vector3Int gridPositionV3Int;

    private void Awake()
    {
        levelGenerator = FindObjectOfType<LevelGenerator>();
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            levelGenerator.UpdateLatestPointOfOptimalPath(gridPositionV3Int);
        }
    }
}
