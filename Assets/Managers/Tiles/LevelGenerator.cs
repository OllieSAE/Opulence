using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap currentTilemap;
    [SerializeField] private TileBase currentTile;

    [Header("Use EVEN numbers!")]
    [SerializeField] public int levelHeight;
    [SerializeField] public int levelWidth;

    [Header("0,0 = full - 1,1 = empty")]
    [SerializeField] public float perlinThresholdMin;
    [SerializeField] public float perlinThresholdMax;

    [Header("Non Procedural Areas")] public List<NonProceduralArea> nonProceduralAreas = new List<NonProceduralArea>();
    [SerializeField] private Camera cam;

    private bool borderGenerated = false;
    private bool spawnedNonProceduralAreas = false;
    private void Update()
    {
        Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

        if (Input.GetMouseButtonDown(0))
        {
            print("generate tiles");
            GenerateTiles();
            //PlaceTile(pos);
        }

        if (Input.GetMouseButton(1))
        {
            ClearTiles();
            //DeleteTile(pos);
        }
    }

    private void Start()
    {
        if (cam == null) cam = FindObjectOfType<Camera>();
        
    }

    private void GenerateTiles()
    {
        for (int x = -levelWidth/2; x < levelWidth/2; x++)
        {
            for (int y = -levelHeight/2; y < levelHeight/2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (Mathf.PerlinNoise(Random.Range(0f, 100f), Time.time) >
                    Random.Range(perlinThresholdMin, perlinThresholdMax))
                {
                    currentTilemap.SetTile(pos,currentTile);
                }
            }
        }
        if (!borderGenerated)
        {
            GenerateBorder();
        }

        if (!spawnedNonProceduralAreas)
        {
            SpawnNonProceduralAreas();
        }
    }

    private void SpawnNonProceduralAreas()
    {
        foreach (NonProceduralArea nonProceduralArea in nonProceduralAreas)
        {
            int tempSpawnPosX = (int)nonProceduralArea.spawnPosition.x;
            int tempSpawnPosY = (int)nonProceduralArea.spawnPosition.y;
            for (int x = -nonProceduralArea.width / 2; x < nonProceduralArea.width / 2; x++)
            {
                for (int y = -nonProceduralArea.height / 2; y < nonProceduralArea.height / 2; y++)
                {
                    Vector3Int pos = new Vector3Int(x+tempSpawnPosX, y+tempSpawnPosY, 0);
                    currentTilemap.SetTile(pos,null);
                }
            }
        }

        spawnedNonProceduralAreas = true;
    }
    
    private void GenerateBorder()
    {
        //left and right borders
        for (int y = -levelHeight / 2; y <= levelHeight / 2; y++)
        {
            Vector3Int posLeft = new Vector3Int((-levelWidth/2), y, 0);
            currentTilemap.SetTile(posLeft,currentTile);
            Vector3Int posRight = new Vector3Int((levelWidth/2), y, 0);
            currentTilemap.SetTile(posRight,currentTile);
        }
        
        //for top and bottom borders
        for (int x = -levelWidth / 2; x <= levelWidth / 2; x++)
        {
            Vector3Int posBottom = new Vector3Int(x, (-levelHeight/2), 0);
            currentTilemap.SetTile(posBottom,currentTile);
            Vector3Int posTop = new Vector3Int(x, (levelHeight/2), 0);
            currentTilemap.SetTile(posTop,currentTile);
        }
        borderGenerated = true;
    }

    private void ClearTiles()
    {
        borderGenerated = false;
        spawnedNonProceduralAreas = false;
        currentTilemap.ClearAllTiles();
    }

    private void PlaceTile(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, currentTile);
    }

    private void DeleteTile(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, null);
    }
}
