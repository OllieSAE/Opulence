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
    [SerializeField] private List<Tilemap> tilemapList = new List<Tilemap>();
    [SerializeField] private CustomTile currentTile;
    [SerializeField] private List<CustomTile> customTiles;
    [SerializeField] private Grid grid;
    public bool gizmosOn;
    [Header("Tile Types")] private CustomTile enemyTile, spikeTile, ruleTile, collectibleTile;

    private Vector3Int[,] tileArrayVector3Ints;
    private CustomTile[,] tile2DArray;
    private Node[,] gridNodeReferences;
    private List<Node> blockedNodes;
    private List<Node> fullNeighbours = new List<Node>();
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
        tile2DArray = new CustomTile[levelHeight, levelWidth];
        gridNodeReferences = new Node[levelHeight+1, levelWidth+1];
        blockedNodes = new List<Node>();
        scale = Random.Range(0.15f, 0.25f);
    }

    private void Update()
    {
        scale = Random.Range(0.15f, 0.25f);
        //or change the 10000 
        
        Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

        //ClearTiles();
        //GenerateTiles();
        
        
        
        if (Input.GetMouseButtonDown(2))
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            //StartCoroutine(FillIndividualEmpty());
            FillIndividualEmpty();
        }
    }

    private void Start()
    {
        if (cam == null) cam = FindObjectOfType<Camera>();
        spikeTile = customTiles.Find(t => t.tileType == CustomTile.TileType.spike);
        enemyTile = customTiles.Find(t => t.tileType == CustomTile.TileType.enemy);
        tilemapList.Add(currentTilemap);
    }

    private void OnDrawGizmos()
    {
        foreach (Node blockedNode in blockedNodes)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(blockedNode.gridPositionGizmosOnly,Vector3.one);
        }

        foreach (Node fullNeighbour in fullNeighbours)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(fullNeighbour.gridPositionGizmosOnly,Vector3.one);
        }
        
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
                
                //list of grid positions that could exist, eg -width/2
                //tilemap.GetTile(position) - true = blocked, false = not blocked
                
                
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
        else GenerateGridNodes();
    }

    private void GenerateGridNodes()
    {
        //could change -levelWidth/2 to be:
        //Vector2 worldBottomLeft = transform.position - Vector2.right * levelWidth - Vector2.up * levelHeight
        //then set X/Y to worldBottomLeft.x/.y respectively
        
        for (int x = (-levelWidth / 2); x < (levelWidth / 2) + 1; x++)
        {
            for (int y = (-levelHeight / 2); y < (levelHeight / 2) + 1; y++)
            {
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2] = new Node();
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].xPosInArray = x;
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].yPosInArray = y;
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].gridPosition = new Vector3(x, y, 0);
                
                var vector2 = new Vector2(x, y);
                Vector3Int location = new Vector3Int(x, y, 0); 
                
                //need to loop through all tilemaps to check any prefab areas
                //for some reason this is not working
                //it's not adding the prefab area to the blockedNodes list
                foreach (Tilemap tilemap in tilemapList)
                {
                    if (tilemap.GetTile(location))
                    {
                        gridNodeReferences[x + levelWidth/2, y + levelHeight/2].isTile = true;
                        blockedNodes.Add(gridNodeReferences[x + levelWidth/2,y + levelHeight/2]);
                    }
                }
            }
        }

        AssignNeighbours();
        
    }

    private void ScanTile(Node node)
    {
        foreach (Tilemap tilemap in tilemapList)
        {
            if (tilemap.GetTile(node.gridPosV3Int)) node.isTile = true;
        }
    }

    private void AssignNeighbours()
    {
        int sizeX = (levelWidth / 2) + 1;
        int sizeY = (levelHeight / 2) + 1;
        int floorX = (-levelWidth / 2);
        int floorY = (-levelHeight / 2);
        int offsetX = levelWidth / 2;
        int offsetY = levelHeight / 2;
        for (int x = floorX; x < sizeX; x++)
        {
            for (int y = floorY; y < sizeY; y++)
            {
                if (x > floorX) gridNodeReferences[x + offsetX-1, y + offsetY].neighbours[2,1] = gridNodeReferences[x + offsetX, y + offsetY];
                if (y > floorY) gridNodeReferences[x + offsetX, y + offsetY-1].neighbours[1,2] = gridNodeReferences[x + offsetX, y + offsetY];
                if (x < sizeX-1) gridNodeReferences[x + offsetX+1, y + offsetY].neighbours[0,1] = gridNodeReferences[x + offsetX, y + offsetY];
                if (y < sizeY-1) gridNodeReferences[x + offsetX, y + offsetY+1].neighbours[1,0] = gridNodeReferences[x + offsetX, y + offsetY];
                if (x > floorX && y > floorY) gridNodeReferences[x + offsetX-1, y + offsetY-1].neighbours[2,2] = gridNodeReferences[x + offsetX, y + offsetY];
                if (x > floorX && y < sizeY-1) gridNodeReferences[x + offsetX-1, y + offsetY+1].neighbours[2,0] = gridNodeReferences[x + offsetX, y + offsetY];
                if (x < sizeX-1 && y > floorY) gridNodeReferences[x + offsetX+1, y + offsetY-1].neighbours[0,2] = gridNodeReferences[x + offsetX, y + offsetY];
                if (x < sizeX-1 && y < sizeY-1) gridNodeReferences[x + offsetX+1, y + offsetY+1].neighbours[0,0] = gridNodeReferences[x + offsetX, y + offsetY];
            }
        }
    }

    private void FillIndividualEmpty()
    {
        fullNeighbours.Clear();

        foreach (Node node in gridNodeReferences)
        {
            int neighboursWithTile = 0;
            if (!node.isTile)
            {
                foreach (Node neighbour in node.neighbours)
                {
                    if (neighbour != null && neighbour.isTile)
                    {
                        neighboursWithTile++;
                        if (neighboursWithTile >= 5)
                        {
                            fullNeighbours.Add(node);
                        }
                    }
                }
            }
        }

        foreach (Node node in fullNeighbours)
        {
            currentTilemap.SetTile(node.gridPosV3Int,currentTile.tile);
        }

        foreach (Node node in gridNodeReferences)
        {
            ScanTile(node);
        }
    }

    private void SpawnNonProceduralAreas()
    {
        //TODO:
        //needs a rework - there's some weird ghost shit going on with the center of the map
        
        /*foreach (NonProceduralArea nonProceduralArea in nonProceduralAreas)
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
                tilemapList.Add(nonProceduralArea.prefab.GetComponent<Tilemap>());
            }
        }*/

        spawnedNonProceduralAreas = true;
        GenerateGridNodes();
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
        blockedNodes.Clear();
        fullNeighbours.Clear();
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
