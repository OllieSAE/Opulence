using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap currentTilemap;
    [SerializeField] private CustomTile currentTile;
    [SerializeField] private List<CustomTile> customTiles;
    [SerializeField] private Grid grid;
    public bool gizmosOn;
    [Header("Tile Types")] private CustomTile enemyTile, spikeTile, ruleTile, collectibleTile;

    private Vector3Int[,] tileArrayVector3Ints;
    private List<CustomTile> spawnedTiles = new List<CustomTile>();
    [Header("Use EVEN numbers!")]
    [SerializeField] public int levelHeight;
    [SerializeField] public int levelWidth;

    [Header("0,0 = full - 1,1 = empty")]
    [SerializeField] public float perlinThresholdMin;
    [SerializeField] public float perlinThresholdMax;
    [SerializeField] public float scale;
    private float previousPerlinValue;
    private int enemySpawnCounter;

    [Header("Non Procedural Areas")] public List<NonProceduralArea> nonProceduralAreas = new List<NonProceduralArea>();
    [SerializeField] private Camera cam;

    private bool borderGenerated = false;
    private bool spawnedNonProceduralAreas = false;

    private void Awake()
    {
        tileArrayVector3Ints = new Vector3Int[levelHeight, levelWidth];
    }

    private void Update()
    {
        Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

        //ClearTiles();
        //GenerateTiles();
        scale = Random.Range(0.15f, 0.25f);
        //or change the 10000 
        
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
        spikeTile = customTiles.Find(t => t.tileType == CustomTile.TileType.spike);
        enemyTile = customTiles.Find(t => t.tileType == CustomTile.TileType.enemy);
    }

    private void OnDrawGizmos()
    {
        if (gizmosOn)
        {
            for (int x = -levelWidth/2; x < levelWidth/2; x++)
            {
                for (int y = -levelHeight/2; y < levelHeight/2; y++)
                {
                    //DELIBERATELY have the XY back to front so it draws horizontally first
                    Vector3Int pos = new Vector3Int(y, x, 0);
                    

                    float perlinNoise = Mathf.PerlinNoise(10000+x*scale,10000+y*scale);
                    float perlinDiff = perlinNoise - previousPerlinValue;
                    if (perlinDiff < 0.15f && perlinDiff > -0.15f)
                    {
                        enemySpawnCounter++;
                        perlinNoise = previousPerlinValue;
                    
                        if (perlinNoise > perlinThresholdMax)
                        {
                    
                            //currentTilemap.SetTile(pos,currentTile);
                            Color tempColor = new Color(perlinNoise, perlinNoise, perlinNoise, 1);
                            //currentTilemap.SetColor(pos,tempColor);
                    
                            Gizmos.color = tempColor;
                            Gizmos.DrawCube(pos,Vector3.one);

                            if (perlinNoise < perlinThresholdMin + perlinThresholdMax)
                            {
                                if (Random.Range(0, 1f) > 0.8f)
                                {
                                    Gizmos.color = Color.red;
                                    Gizmos.DrawCube(pos,Vector3.one);
                                }
                                if (enemySpawnCounter > 3)
                                {
                                    enemySpawnCounter = 0;
                                    Gizmos.color = Color.blue;
                                    Gizmos.DrawCube(pos,Vector3.one);
                                }
                            }

                        
                        }
                    }
                
                    previousPerlinValue = perlinNoise;
                }
            }
        }
    }

    private void GenerateTiles()
    {
        for (int x = -levelWidth/2; x < levelWidth/2; x++)
        {
            for (int y = -levelHeight/2; y < levelHeight/2; y++)
            {
                //DELIBERATELY have the XY back to front so it draws horizontally first
                Vector3Int pos = new Vector3Int(y, x, 0);
                
                //needs to account for negative
                //tileArrayVector3Ints[y, x] = pos;

                float perlinNoise = Mathf.PerlinNoise(10000+x*scale,10000+y*scale);
                float perlinDiff = perlinNoise - previousPerlinValue;
                if (perlinDiff < 0.15f && perlinDiff > -0.15f)
                {
                    enemySpawnCounter++;
                    perlinNoise = previousPerlinValue;
                    
                    if (perlinNoise > perlinThresholdMax)
                    {
                    
                        //this gets overriden each iteration through the loop
                        //need to store it as a position separately to the SO
                        //dont think SO can store its own value because they're not separate objects
                        //alternatively, look at how the waterNode grid worked in Yougenics!
                        currentTile.AssignPosition(x,y);
                        
                        currentTilemap.SetTile(pos,currentTile.tile);
                        spawnedTiles.Add(currentTile);
                        
                        
                        //Color tempColor = new Color(perlinNoise, perlinNoise, perlinNoise, 1);
                        //Gizmos.color = tempColor;
                        //Gizmos.DrawCube(pos,Vector3.one);

                        if (perlinNoise < perlinThresholdMin + perlinThresholdMax)
                        {
                            if (Random.Range(0, 1f) > 0.8f)
                            {
                                currentTilemap.SetTile(pos,spikeTile.tile);

                                //Gizmos.color = Color.red;
                                //Gizmos.DrawCube(pos,Vector3.one);
                            }
                            if (enemySpawnCounter > 3)
                            {
                                enemySpawnCounter = 0;
                                
                                currentTilemap.SetTile(pos,enemyTile.tile);
                                
                                //enemyTile.CheckNeighbours();
                                
                                //Gizmos.color = Color.blue;
                                //Gizmos.DrawCube(pos,Vector3.one);
                            }
                        }

                        
                    }
                }
                
                previousPerlinValue = perlinNoise;
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

        foreach (CustomTile spawnedTile in spawnedTiles)
        {
            spawnedTile.CheckNeighbours();
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

            if (nonProceduralArea.prefab != null)
            {
                Instantiate(nonProceduralArea.prefab,nonProceduralArea.spawnPosition,quaternion.identity,grid.transform);
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
            currentTilemap.SetTile(posLeft,currentTile.tile);
            Vector3Int posRight = new Vector3Int((levelWidth/2), y, 0);
            currentTilemap.SetTile(posRight,currentTile.tile);
        }
        
        //for top and bottom borders
        for (int x = -levelWidth / 2; x <= levelWidth / 2; x++)
        {
            Vector3Int posBottom = new Vector3Int(x, (-levelHeight/2), 0);
            currentTilemap.SetTile(posBottom,currentTile.tile);
            Vector3Int posTop = new Vector3Int(x, (levelHeight/2), 0);
            currentTilemap.SetTile(posTop,currentTile.tile);
        }
        borderGenerated = true;
    }

    private void ClearTiles()
    {
        borderGenerated = false;
        spawnedNonProceduralAreas = false;
        currentTilemap.ClearAllTiles();
        foreach (Transform child in grid.transform)
        {
            child.GetComponent<Tilemap>().ClearAllTiles();
            if (child != currentTilemap.transform) Destroy(child.transform.gameObject);
        }
    }

    private void PlaceTile(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, currentTile.tile);
    }

    private void DeleteTile(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, null);
    }
}
