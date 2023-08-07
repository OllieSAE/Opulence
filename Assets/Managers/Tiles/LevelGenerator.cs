using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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
    [SerializeField] private RuleTile currentTile;
    [SerializeField] private List<CustomTile> customTiles;
    [SerializeField] private Grid grid;
    private EnemySpawner enemySpawner;
    public GameObject player;
    private Pathfinding pathfinding;
    private Node latestOptimalTraversed;
    public GameObject playerRescuePrefab;
    public bool gizmosOn;
    private bool floodingStarted = false;
    [Header("Tile Types")] private CustomTile enemyTile, spikeTile, ruleTile, collectibleTile;

    private Vector3Int[,] tileArrayVector3Ints;
    private CustomTile[,] tile2DArray;
    private Node[,] gridNodeReferences;
    private List<Node> blockedNodes;
    private List<Node> fullNeighbours = new List<Node>();
    private List<Node> outliers = new List<Node>();
    [Header("Use EVEN numbers!")]
    [SerializeField] public int levelHeight;
    [SerializeField] public int levelWidth;

    [Header("0,0 = full - 1,1 = empty")]
    [SerializeField] public float perlinThresholdMin;
    [SerializeField] public float perlinThresholdMax;
    [SerializeField] public float scale;
    private float previousPerlinValue;
    private int enemySpawnCounter;

    public List<Node> reachableTiles = new List<Node>();
    [Header("Non Procedural Areas")] public List<NonProceduralArea> nonProceduralAreas = new List<NonProceduralArea>();
    [SerializeField] private Camera cam;
    
    //testing neighbours
    private Node topLeftNeighbour,
        topRightNeighbour,
        bottomLeftNeighbour,
        bottomRightNeighbour,
        topNeighbour,
        bottomNeighbour,
        leftNeighbour,
        rightNeighbour;

    private bool nodeSelected = false;

    private bool borderGenerated = false;
    private bool borderNodesGenerated = false;
    private bool spawnedNonProceduralAreas = false;
    private bool levelFinishedLoading = false;

    public static LevelGenerator _instance;

    public static LevelGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                print("Level Manager is null");
            }

            return _instance;
        }
    }

    public delegate void ClearTilesEvent();
    public event ClearTilesEvent clearTilesEvent;
    
    private void Awake()
    {
        _instance = this;
        tileArrayVector3Ints = new Vector3Int[levelHeight, levelWidth];
        tile2DArray = new CustomTile[levelHeight, levelWidth];
        enemySpawner = FindObjectOfType<EnemySpawner>();
        blockedNodes = new List<Node>();
        scale = Random.Range(0.15f, 0.25f);
        pathfinding = GetComponent<Pathfinding>();
    }

    private void Start()
    {
        if (cam == null) cam = FindObjectOfType<Camera>();
        spikeTile = customTiles.Find(t => t.tileType == CustomTile.TileType.spike);
        enemyTile = customTiles.Find(t => t.tileType == CustomTile.TileType.enemy);
        tilemapList.Add(currentTilemap);
        pathfinding.restartLevelGenEvent += ClearTiles;
        pathfinding.levelGenSuccessEvent += PathfindingComplete;
        GenerateTiles();
    }
    
    private void Update()
    {
        //move this once respawning causes level to change
        scale = Random.Range(0.15f, 0.25f);
        
        //ScanAllTiles is called several times during the actual generation of level
        //but also placed in update to further ensure that tiles are properly regularly scanned
        //stops once the level is finished (when Enemies get spawned)
        if (!levelFinishedLoading)
        {
            ScanAllTiles();
        }
    }
    
    private void GenerateTiles()
    {
        //deliberately expanding out from 0,0,0, so we're not "hard coding" things to 0. Makes for slightly messier level gen but easier modularity in future designs.
        for (int x = -levelWidth/2; x < levelWidth/2; x++)
        {
            for (int y = -levelHeight/2; y < levelHeight/2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                float perlinNoise = Mathf.PerlinNoise(10000+x*scale,10000+y*scale);
                
                //to encourage longer sets of straight lines, the noise is homogenised if it's close to the previous value
                float perlinDiff = perlinNoise - previousPerlinValue;
                if (perlinDiff < 0.15f && perlinDiff > -0.15f)
                {
                    perlinNoise = previousPerlinValue;
                    
                    if (perlinNoise > perlinThresholdMax)
                    {
                        currentTilemap.SetTile(pos,currentTile);
                    }
                }
                previousPerlinValue = perlinNoise;
            }
        }
        if (!borderGenerated)
        {
            //forces a continuous border surrounding the level. Only allows for square levels at the moment.
            GenerateBorder();
        }
        
        GenerateGridNodes();
    }
    
    private void ScanAllTiles()
    {
        foreach (Node node in gridNodeReferences)
        {
            if (currentTilemap.GetTile(node.gridPosV3Int))
            {
                node.isTile = true;
                node.isReachable = false;
            }
            else node.isTile = false;
        }
    }
    
    private void ScanTile(Node node)
    {
        foreach (Tilemap tilemap in tilemapList)
        {
            if (tilemap.GetTile(node.gridPosV3Int))
            {
                node.isTile = true;
                node.isReachable = false;
            }
            else 
            //this part looks extremely chaotic, (and it is) but in short, it's setting a node to reachable, if certain parameters are met in regards to it's neighbours
            //however, due to the neighbours only existing in a 3x3 grid around the node, it gets more and more convoluted as it checks it's neighbours' neighbours, etc
            //this does work, and allows the pathfinder to treat the nodes in a similar to way to how a player would jump/climb through the level.
            //unfortunately, this is NOT perfect. The failsafe exists further on, where extra tiles are thrown adjacent to the optimal path, which forces the level to be completable.
            //Despite not being perfect, it has allowed for a better platform-y feel to the levels, so it's a net positive overall!
            {
                node.isTile = false;
                //[1,0] is directly beneath
                if (node.neighbours[1, 0] != null)
                {
                    if(node.neighbours[1, 0].isTile) node.isReachable = true;
                    else if (node.neighbours[1, 0].neighbours[1, 0] != null)
                    {
                        if(node.neighbours[1, 0].neighbours[1, 0].isTile) node.isReachable = true;
                        else if (node.neighbours[1, 0].neighbours[1, 0].neighbours[1, 0] != null &&
                                 node.neighbours[1, 0].neighbours[1, 0].neighbours[1, 0].isTile) node.isReachable = true;
                    }
                }

                //if left or right are tiles, then node is traversable (ie, wall jump)
                if (node.neighbours[0, 1] != null && node.neighbours[0, 1].isTile) node.isReachable = true;
                if (node.neighbours[2, 1] != null && node.neighbours[2, 1].isTile) node.isReachable = true;

                //one jump right, via diagonally up to the right then diagonally down to the right
                if (node.neighbours[2, 2] != null && node.neighbours[2, 1] != null && node.neighbours[2, 0] != null)
                {
                    if (!node.neighbours[2, 2].isTile && !node.neighbours[2, 1].isTile && !node.neighbours[2, 0].isTile)
                    {
                        if ((node.neighbours[2, 1].neighbours[2, 0].isTile &&
                            node.neighbours[2, 1].neighbours[2, 0] != null)&& (!node.neighbours[2,1].neighbours[2,1].isTile && node.neighbours[2,1].neighbours[2,1] != null))
                        {
                            node.neighbours[2, 2].isReachable = true;
                            node.neighbours[2, 1].neighbours[2, 1].isReachable = true;
                        }
                    }
                }
                
                //two jump right, via diagonally up to the right, then right then diagonally down
                if (node.neighbours[2, 2] != null && node.neighbours[2, 1] != null && node.neighbours[2, 0] != null)
                {
                    if (!node.neighbours[2, 2].isTile && !node.neighbours[2, 1].isTile && !node.neighbours[2, 0].isTile)
                    {
                        if (node.neighbours[2, 1].neighbours[2, 2] != null && node.neighbours[2, 1].neighbours[2, 1] != null && node.neighbours[2, 1].neighbours[2, 0] != null)
                        {
                            if (!node.neighbours[2, 1].neighbours[2, 2].isTile && !node.neighbours[2, 1].neighbours[2, 1].isTile && !node.neighbours[2, 1].neighbours[2, 0].isTile)
                            {
                                if ((node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 0].isTile &&
                                     node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 0] != null) &&
                                    (!node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].isTile &&
                                     node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1] != null))
                                {
                                    node.neighbours[2, 2].isReachable = true;
                                    node.neighbours[2, 1].neighbours[2, 2].isReachable = true;
                                    node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].isReachable = true;
                                }
                            }
                        }
                    }
                }
                
                //three jump right, via diagonally up to the right, then right twice, then diagonally down
                if (node.neighbours[2, 2] != null && node.neighbours[2, 1] != null && node.neighbours[2, 0] != null)
                {
                    if (!node.neighbours[2, 2].isTile && !node.neighbours[2, 1].isTile && !node.neighbours[2, 0].isTile)
                    {
                        if (node.neighbours[2, 1].neighbours[2, 2] != null && node.neighbours[2, 1].neighbours[2, 1] != null && node.neighbours[2, 1].neighbours[2, 0] != null)
                        {
                            if (!node.neighbours[2, 1].neighbours[2, 2].isTile && !node.neighbours[2, 1].neighbours[2, 1].isTile && !node.neighbours[2, 1].neighbours[2, 0].isTile)
                            {
                                if (node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 2] != null && node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1] != null && node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 0] != null)
                                {
                                    if (!node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 2].isTile && !node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 2].isTile && !node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 2].isTile)
                                    {
                                        if ((node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].neighbours[2, 0].isTile &&
                                             node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].neighbours[2, 0] != null) &&
                                            (!node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].isTile &&
                                             node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].neighbours[2, 1] != null))
                                        {
                                            node.neighbours[2, 2].isReachable = true;
                                            node.neighbours[2, 1].neighbours[2, 2].isReachable = true;
                                            node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 2].isReachable = true;
                                            node.neighbours[2, 1].neighbours[2, 1].neighbours[2, 1].neighbours[2, 1]
                                                .isReachable = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                //one jump left, via diagonally up to the left, then diagonally down to the left
                if (node.neighbours[0, 2] != null && node.neighbours[0, 1] != null && node.neighbours[0, 0] != null)
                {
                    if (!node.neighbours[0, 2].isTile && !node.neighbours[0, 1].isTile && !node.neighbours[0, 0].isTile)
                    {
                        if ((node.neighbours[0, 1].neighbours[0, 0].isTile &&
                             node.neighbours[0, 1].neighbours[0, 0] != null)&& (!node.neighbours[0,1].neighbours[0,1].isTile && node.neighbours[0,1].neighbours[0,1] != null))
                        {
                            node.neighbours[0, 2].isReachable = true;
                            node.neighbours[0, 1].neighbours[0, 1].isReachable = true;
                        }
                    }
                }
                
                //two jump left, via diagonally up to the left, then left once, then diagonally down to the left
                if (node.neighbours[0, 2] != null && node.neighbours[0, 1] != null && node.neighbours[0, 0] != null)
                {
                    if (!node.neighbours[0, 2].isTile && !node.neighbours[0, 1].isTile && !node.neighbours[0, 0].isTile)
                    {
                        if (node.neighbours[0, 1].neighbours[0, 2] != null && node.neighbours[0, 1].neighbours[0, 1] != null && node.neighbours[0, 1].neighbours[0, 0] != null)
                        {
                            if (!node.neighbours[0, 1].neighbours[0, 2].isTile && !node.neighbours[0, 1].neighbours[0, 1].isTile && !node.neighbours[0, 1].neighbours[0, 0].isTile)
                            {
                                if ((node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 0].isTile &&
                                     node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 0] != null) &&
                                    (!node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].isTile &&
                                     node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1] != null))
                                {
                                    node.neighbours[0, 2].isReachable = true;
                                    node.neighbours[0, 1].neighbours[0, 2].isReachable = true;
                                    node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].isReachable = true;
                                }
                            }
                        }
                    }
                }
                
                //three jump left, via diagonally up to the left, then left twice, then diagonally down to the left
                if (node.neighbours[0, 2] != null && node.neighbours[0, 1] != null && node.neighbours[0, 0] != null)
                {
                    if (!node.neighbours[0, 2].isTile && !node.neighbours[0, 1].isTile && !node.neighbours[0, 0].isTile)
                    {
                        if (node.neighbours[0, 1].neighbours[0, 2] != null && node.neighbours[0, 1].neighbours[0, 1] != null && node.neighbours[0, 1].neighbours[0, 0] != null)
                        {
                            if (!node.neighbours[0, 1].neighbours[0, 2].isTile && !node.neighbours[0, 1].neighbours[0, 1].isTile && !node.neighbours[0, 1].neighbours[0, 0].isTile)
                            {
                                if (node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 2] != null && node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1] != null && node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 0] != null)
                                {
                                    if (!node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 2].isTile && !node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 2].isTile && !node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 2].isTile)
                                    {
                                        if ((node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].neighbours[0, 0].isTile &&
                                             node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].neighbours[0, 0] != null) &&
                                            (!node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].isTile &&
                                             node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].neighbours[0, 1] != null))
                                        {
                                            node.neighbours[0, 2].isReachable = true;
                                            node.neighbours[0, 1].neighbours[0, 2].isReachable = true;
                                            node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 2].isReachable = true;
                                            node.neighbours[0, 1].neighbours[0, 1].neighbours[0, 1].neighbours[0, 1]
                                                .isReachable = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else node.isReachable = false;
            }
        }
    }
    
    private void GenerateBorder()
    {
        //left and right borders + 1, 2 and 3 offset
        for (int y = -levelHeight / 2 - 3; y <= levelHeight / 2 + 3; y++)
        {
            Vector3Int posLeft = new Vector3Int((-levelWidth/2), y, 0);
            currentTilemap.SetTile(posLeft,currentTile);
            Vector3Int posLeft1 = new Vector3Int((-levelWidth / 2 - 1), y, 0);
            currentTilemap.SetTile(posLeft1,currentTile);
            Vector3Int posLeft2 = new Vector3Int((-levelWidth / 2 - 2), y, 0);
            currentTilemap.SetTile(posLeft2,currentTile);
            Vector3Int posLeft3 = new Vector3Int((-levelWidth / 2 - 3), y, 0);
            currentTilemap.SetTile(posLeft3,currentTile);
            Vector3Int posRight = new Vector3Int((levelWidth/2), y, 0);
            currentTilemap.SetTile(posRight,currentTile);
            Vector3Int posRight1 = new Vector3Int((levelWidth/2 + 1), y, 0);
            currentTilemap.SetTile(posRight1,currentTile);
            Vector3Int posRight2 = new Vector3Int((levelWidth/2 + 2), y, 0);
            currentTilemap.SetTile(posRight2,currentTile);
            Vector3Int posRight3 = new Vector3Int((levelWidth/2 + 3), y, 0);
            currentTilemap.SetTile(posRight3,currentTile);
        }
        
        //for top and bottom borders + 1, 2 and 3 offset
        for (int x = -levelWidth / 2 - 3; x <= levelWidth / 2 + 3; x++)
        {
            Vector3Int posBottom = new Vector3Int(x, (-levelHeight/2), 0);
            currentTilemap.SetTile(posBottom,currentTile);
            Vector3Int posBottom1 = new Vector3Int(x, (-levelHeight/2 - 1), 0);
            currentTilemap.SetTile(posBottom1,currentTile);
            Vector3Int posBottom2 = new Vector3Int(x, (-levelHeight/2 - 2), 0);
            currentTilemap.SetTile(posBottom2,currentTile);
            Vector3Int posBottom3 = new Vector3Int(x, (-levelHeight/2 - 3), 0);
            currentTilemap.SetTile(posBottom3,currentTile);
            Vector3Int posTop = new Vector3Int(x, (levelHeight/2), 0);
            currentTilemap.SetTile(posTop,currentTile);
            Vector3Int posTop1 = new Vector3Int(x, (levelHeight/2 + 1), 0);
            currentTilemap.SetTile(posTop1,currentTile);
            Vector3Int posTop2 = new Vector3Int(x, (levelHeight/2 + 2), 0);
            currentTilemap.SetTile(posTop2,currentTile);
            Vector3Int posTop3 = new Vector3Int(x, (levelHeight/2 + 3), 0);
            currentTilemap.SetTile(posTop3,currentTile);
        }
        borderGenerated = true;
    }
    
    private void GenerateGridNodes()
    {
        gridNodeReferences = null; //null at the start of generation each time to prevent corruption on repeated level gen
        gridNodeReferences = new Node[levelHeight+1, levelWidth+1];
        for (int x = (-levelWidth / 2); x < (levelWidth / 2) + 1; x++)
        {
            for (int y = (-levelHeight / 2); y < (levelHeight / 2) + 1; y++)
            {
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2] = new Node();
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].xPosInArray = x + levelWidth/2;
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].yPosInArray = y + levelHeight/2;
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].gridPosition = new Vector3(x, y, 0);
                
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
                //This is messy. It looks gross. But it works. /shrug
                if (x > floorX) gridNodeReferences[x + offsetX-1, y + offsetY].neighbours[2,1] = gridNodeReferences[x + offsetX, y + offsetY]; //left edge
                if (y > floorY) gridNodeReferences[x + offsetX, y + offsetY-1].neighbours[1,2] = gridNodeReferences[x + offsetX, y + offsetY]; //bottom edge 
                if (x < sizeX-1) gridNodeReferences[x + offsetX+1, y + offsetY].neighbours[0,1] = gridNodeReferences[x + offsetX, y + offsetY]; //right edge
                if (y < sizeY-1) gridNodeReferences[x + offsetX, y + offsetY+1].neighbours[1,0] = gridNodeReferences[x + offsetX, y + offsetY]; //top edge
                if (x > floorX && y > floorY) gridNodeReferences[x + offsetX-1, y + offsetY-1].neighbours[2,2] = gridNodeReferences[x + offsetX, y + offsetY]; //bottom left corner
                if (x > floorX && y < sizeY-1) gridNodeReferences[x + offsetX-1, y + offsetY+1].neighbours[2,0] = gridNodeReferences[x + offsetX, y + offsetY]; //top left corner
                if (x < sizeX-1 && y > floorY) gridNodeReferences[x + offsetX+1, y + offsetY-1].neighbours[0,2] = gridNodeReferences[x + offsetX, y + offsetY]; //bottom right corner
                if (x < sizeX-1 && y < sizeY-1) gridNodeReferences[x + offsetX+1, y + offsetY+1].neighbours[0,0] = gridNodeReferences[x + offsetX, y + offsetY]; //top right corner
            }
        }

        ScanAllTiles();
        
        foreach (Node node in gridNodeReferences)
        {
            ScanTile(node);
        }
        
        GenerateBorderNodes();
        
        FillIndividualEmpty();
        
    }
    
    public void FloodFill(int x, int y)
    {
        if (floodingStarted == false) StartCoroutine(FillNonReachable());
        floodingStarted = true;
        if (x > -levelWidth/2 && x < levelWidth/2 && y > -levelHeight/2 && y < levelHeight/2)
        {
            if (!gridNodeReferences[x + levelWidth / 2, y + levelHeight / 2].isTile)
            {
                if (!reachableTiles.Contains(gridNodeReferences[x + levelWidth / 2, y + levelHeight / 2]))
                {
                    reachableTiles.Add(gridNodeReferences[x + levelWidth / 2, y + levelHeight / 2]);
                    FloodFill(x + 1, y);
                    FloodFill(x - 1, y);
                    FloodFill(x, y + 1);
                    FloodFill(x, y - 1);
                }
            }
        }
    }

    public IEnumerator FillNonReachable()
    {
        yield return new WaitForSeconds(1f);
        foreach (Node node in gridNodeReferences)
        {
            if (!reachableTiles.Contains(node))
            {
                currentTilemap.SetTile(node.gridPosV3Int,currentTile);
            }
        }
        ScanAllTiles();
        yield return new WaitForSeconds(0.1f);
        SetEnemyTiles();
    }

    public int MaxSize
    {
        get
        {
            return levelHeight * levelWidth;
        }
    }

    

    

    private void OnDisable()
    {
        pathfinding.restartLevelGenEvent -= ClearTiles;
        pathfinding.levelGenSuccessEvent -= PathfindingComplete;
    }

    private void PathfindingComplete()
    {
        BlockInPath();
    }

    private void BlockInPath()
    {
        //refactor this to take in OPTIMAL PATH and do another for SECONDARY PATH
        if (optimalPath != null)
        {
            for (int i = 1; i < optimalPath.Count-1; i++)
            {
                //if next step of path is to my RIGHT, and previous is NOT BELOW current, put tile below current
                if (optimalPath[i + 1] == optimalPath[i].neighbours[2, 1] && optimalPath[i-1] != optimalPath[i].neighbours[1,0])
                {
                    currentTilemap.SetTile(optimalPath[i].neighbours[1,0].gridPosV3Int,currentTile);
                }
                
                //if next step of path is to my LEFT, and previous is NOT BELOW current, put tile below current
                if (optimalPath[i + 1] == optimalPath[i].neighbours[0, 1] && optimalPath[i-1] != optimalPath[i].neighbours[1,0])
                {
                    currentTilemap.SetTile(optimalPath[i].neighbours[1,0].gridPosV3Int,currentTile);
                }

                //if next step of path is ABOVE && previous step is BELOW
                if (optimalPath[i + 1] == optimalPath[i].neighbours[1, 2] && optimalPath[i-1] == optimalPath[i].neighbours[1,0])
                {
                    //if no tile to left, add tile to right
                    if(optimalPath[i].neighbours[0,1].isTile == false) currentTilemap.SetTile(optimalPath[i].neighbours[2,1].gridPosV3Int,currentTile);
                    //if no tile to right, add tile to left
                    else if (optimalPath[i].neighbours[2, 1].isTile == false)
                        currentTilemap.SetTile(optimalPath[i].neighbours[0, 1].gridPosV3Int, currentTile);
                }
                
                //if next step is ABOVE and previous step is LEFT or RIGHT
                if(optimalPath[i+1] == optimalPath[i].neighbours[1,2] && (optimalPath[i-1] == optimalPath[i].neighbours[0,1] || optimalPath[i-1] == optimalPath[i].neighbours[2,1]))
                {
                    //if no tile below, set tile below
                    if(optimalPath[i].neighbours[1,0].isTile == false) currentTilemap.SetTile(optimalPath[i].neighbours[1,0].gridPosV3Int,currentTile);
                }
                    
            }
        }

        if (secondaryPath != null && optimalPath != null)
        {
            for (int i = 1; i < secondaryPath.Count-1; i++)
            {
                //if next step of path is to my RIGHT, and previous is NOT BELOW current, put tile below current
                if (secondaryPath[i + 1] == secondaryPath[i].neighbours[2, 1] && secondaryPath[i-1] != secondaryPath[i].neighbours[1,0])
                {
                    if(!optimalPath.Contains(secondaryPath[i].neighbours[1,0])) currentTilemap.SetTile(secondaryPath[i].neighbours[1,0].gridPosV3Int,currentTile);
                }
                
                //if next step of path is to my LEFT, and previous is NOT BELOW current, put tile below current
                if (secondaryPath[i + 1] == secondaryPath[i].neighbours[0, 1] && secondaryPath[i-1] != secondaryPath[i].neighbours[1,0])
                {
                    if(!optimalPath.Contains(secondaryPath[i].neighbours[1,0])) currentTilemap.SetTile(secondaryPath[i].neighbours[1,0].gridPosV3Int,currentTile);
                }

                //if next step of path is ABOVE && previous step is BELOW
                if (secondaryPath[i + 1] == secondaryPath[i].neighbours[1, 2] && secondaryPath[i-1] == secondaryPath[i].neighbours[1,0])
                {
                    //if no tile to right, add tile to left (as long as it's not in optimal path)
                    if (secondaryPath[i].neighbours[2, 1].isTile == false)
                    {
                        if(!optimalPath.Contains(secondaryPath[i].neighbours[2,1])) currentTilemap.SetTile(secondaryPath[i].neighbours[2,1].gridPosV3Int,currentTile);
                    }
                    //if no tile to left, add tile to right (as long as it's not in optimal path)
                    else if (secondaryPath[i].neighbours[0, 1].isTile == false)
                    {
                        if(!optimalPath.Contains(secondaryPath[i].neighbours[0,1])) currentTilemap.SetTile(secondaryPath[i].neighbours[0, 1].gridPosV3Int, currentTile);
                    }
                }
                
                //if next step is ABOVE and previous step is LEFT or RIGHT
                if(secondaryPath[i+1] == secondaryPath[i].neighbours[1,2] && (secondaryPath[i-1] == secondaryPath[i].neighbours[0,1] || secondaryPath[i-1] == secondaryPath[i].neighbours[2,1]))
                {
                    //if no tile below, set tile below
                    if(secondaryPath[i].neighbours[1,0].isTile == false && !optimalPath.Contains(secondaryPath[i].neighbours[1,0])) currentTilemap.SetTile(secondaryPath[i].neighbours[1,0].gridPosV3Int,currentTile);
                }
            }
        }

        ScanAllTiles();
        //if(optimalPath!=null) StartCoroutine(FloodFill(optimalPath[1].gridPosV3Int.x, optimalPath[1].gridPosV3Int.y));
        if(optimalPath!=null) FloodFill(optimalPath[1].gridPosV3Int.x, optimalPath[1].gridPosV3Int.y);
    }

    
    public void SetEnemyTiles()
    {
        foreach (Node node in gridNodeReferences)
        {
            //if node is a tile && not on the bottom - so it doesn't spawn enemies right next to the player at the beginning
            if (node.isTile && node.gridPosition.y > -levelHeight/2)
            {
                //if left & right neighbour aren't null
                if (node.neighbours[0, 1] != null && node.neighbours[2, 1] != null)
                {
                    //if left & right neighbour are tiles, and LEFT is not an enemy tile
                    if (node.neighbours[0, 1].isTile && !node.neighbours[0,1].enemySpawner && node.neighbours[2, 1].isTile)
                    {
                        //if northwest, north and northeast neighbour aren't null
                        if (node.neighbours[0, 2] != null && node.neighbours[1, 2] != null  && node.neighbours[2, 2] != null )
                        {
                            //if northwest, north and northeast neighbour aren't tiles
                            if (!node.neighbours[0, 2].isTile && !node.neighbours[1, 2].isTile &&
                                !node.neighbours[2, 2].isTile)
                            {
                                //set to enemy tile
                                
                                //TODO
                                //this might need to be a separate rule tile?
                                currentTilemap.SetTile(node.gridPosV3Int,currentTile);
                                node.enemySpawner = true;
                                enemySpawner.enemyNodes.Add(node);
                            }
                        }
                    }
                }
            }
        }

        SetSpikeTiles();
    }

    public void SetSpikeTiles()
    {
        foreach (Node node in gridNodeReferences)
        {
            //copy the format of SetEnemyTiles above
            //for a node that has L and R neighbours, and nothing in the northern row
            //set north neighbour to SPIKES on a random chance
            
            //for a node that has L and R neighbours, and nothing in the southern row
            //set south neighbour to SPIKES on a random chance
            
            //for a node that has N and S neighbours, and nothing in the eastern row
            //set east neighbour to SPIKE on a random chance
            
            //for a node that has N and S neighbours, and nothing in the western row
            //set west neighbour to SPIKE on a random chance
            
            //probably can't have wall spikes cz you won't be able to jump!!!
        }
        
        SpawnNonProceduralAreas();
        SetPlayerRescues();
    }

    public void SetPlayerRescues()
    {
        if (optimalPath != null)
        {
            foreach (Node node in optimalPath)
            {
                GameObject go = Instantiate(playerRescuePrefab,node.gridPositionGizmosOnly,quaternion.identity);
                go.GetComponent<RescueLocation>().gridPositionV3Int = node.gridPosV3Int;
            }
        }
        GameManager.Instance.LevelGenComplete();
    }

    //GameManager calls this when Level is loaded
    public void SpawnEnemies()
    {
        enemySpawner.SpawnEnemies();
        levelFinishedLoading = true;
    }

    public void UpdateLatestPointOfOptimalPath(Vector3Int pos)
    {
        if (optimalPath != null)
        {
            foreach (Node node in optimalPath)
            {
                if (node.gridPosV3Int == pos)
                {
                    latestOptimalTraversed = node;
                }
            }
        }
    }

    [Button("Rescue Player Test")]
    public void RescuePlayer()
    {
        if (latestOptimalTraversed != null)
        {
            player.transform.position = latestOptimalTraversed.gridPositionGizmosOnly;
        }
    }

    public IEnumerator RestartForTesting()
    {
        //stopwatch.Reset();
        //successfulLevels++;
        yield return new WaitForSeconds(1f);
        ClearTiles();
    }

    public List<Node> path;
    public List<Node> optimalPath;
    public List<Node> secondaryPath;
    private void OnDrawGizmos()
    {
        // if (reachableTiles != null)
        // {
        //     foreach (Node node in reachableTiles)
        //     {
        //         Gizmos.color = Color.cyan;
        //         Gizmos.DrawCube(node.gridPositionGizmosOnly, Vector3.one);
        //     }
        // }

        if (latestOptimalTraversed != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(latestOptimalTraversed.gridPositionGizmosOnly, Vector3.one);
        }
        // foreach (Node blockedNode in blockedNodes)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawCube(blockedNode.gridPositionGizmosOnly,Vector3.one);
        // }
        //
        // foreach (Node fullNeighbour in fullNeighbours)
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawCube(fullNeighbour.gridPositionGizmosOnly,Vector3.one);
        // }

        // foreach (Node node in outliers)
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawCube(node.gridPositionGizmosOnly, Vector3.one);
        // }

        // if (borderNodesGenerated)
        // {
        //     foreach (Node node in gridNodeReferences)
        //     {
        //         if (node.borderNode)
        //         {
        //             Gizmos.color = Color.yellow;
        //             Gizmos.DrawCube(node.gridPositionGizmosOnly, Vector3.one);
        //         }
        //     }
        // }
        

        /*if (nodeSelected)
        {
            //top left
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(topLeftNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //top right
            Gizmos.color = Color.red;
            Gizmos.DrawCube(topRightNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //bottom left
            Gizmos.color = Color.green;
            Gizmos.DrawCube(bottomLeftNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //bottom right
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(bottomRightNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //top
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(topNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //bottom
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(bottomNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //left
            Gizmos.color = Color.white;
            Gizmos.DrawCube(leftNeighbour.gridPositionGizmosOnly,Vector3.one);
            
            //right
            Gizmos.color = Color.black;
            Gizmos.DrawCube(rightNeighbour.gridPositionGizmosOnly,Vector3.one);
        }
        */

        /*if (path != null)
        {
            foreach (Node node in path)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.gridPositionGizmosOnly,Vector3.one);
            }
        }

        if (optimalPath != null)
        {
            foreach (Node node in optimalPath)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.gridPositionGizmosOnly,Vector3.one);
            }
        }

        if (secondaryPath != null)
        {
            foreach (Node node in secondaryPath)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.gridPositionGizmosOnly,Vector3.one);
            }
        }*/
        


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

    

    

    public void ClearAroundLowest(Node lowest)
    {
        foreach (Node neighbour in lowest.neighbours)
        {
            if (neighbour != null && !neighbour.borderNode)
            {
                currentTilemap.SetTile(neighbour.gridPosV3Int,null);
                ScanTile(neighbour);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                if (x == -1 && y == -1) continue;
                if (x == -1 && y == 1) continue;
                if (x == 1 && y == -1) continue;
                if (x == 1 && y == 1) continue;

                int checkX = node.xPosInArray + x;
                int checkY = node.yPosInArray + y;

                if (checkX >= 0 && checkX < levelWidth && checkY >= 0 && checkY < levelHeight)
                {
                    neighbours.Add(gridNodeReferences[checkX,checkY]);
                }
            }
        }

        return neighbours;
    }



    

    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        float percentX = (worldPosition.x + levelWidth / 2) / levelWidth;
        float percentY = (worldPosition.y + levelHeight / 2) / levelHeight;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((levelWidth - 1) * percentX);
        int y = Mathf.RoundToInt((levelHeight - 1) * percentY);

        return gridNodeReferences[x, y];
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

        if (fullNeighbours.Count == 0)
        {
            ClearSingleOutliers();
            return;
        }
        
        foreach (Node node in fullNeighbours)
        {
            currentTilemap.SetTile(node.gridPosV3Int,currentTile);
            ScanTile(node);
        }
        
        ScanAllTiles();

        foreach (Node node in gridNodeReferences)
        {
            ScanTile(node);
        }
        FillIndividualEmpty();
    }

    private void ClearSingleOutliers()
    {
        outliers.Clear();
        
        foreach (Node node in gridNodeReferences)
        {
            int neighboursWithTile = 0;
            if (node.isTile)
            {
                foreach (Node neighbour in node.neighbours)
                {
                    if (neighbour != null && neighbour.isTile)
                    {
                        neighboursWithTile++;
                    }
                }
                if (neighboursWithTile <= 3)
                {
                    if(!node.borderNode) outliers.Add(node);
                }
            }
        }

        if (outliers.Count == 0)
        {
            StartCoroutine(MapFinished());
            return;
        }

        foreach (Node node in outliers)
        {
            currentTilemap.SetTile(node.gridPosV3Int,null);
            ScanTile(node);
        }
        
        ScanAllTiles();
        
        foreach (Node node in gridNodeReferences)
        {
            ScanTile(node);
        }

        ClearSingleOutliers();
    }

    private IEnumerator MapFinished()
    {
        
        yield return new WaitForSeconds(0.1f);
        pathfinding.FindDefaultPath();
    }

    private void SpawnNonProceduralAreas()
    {
        //TODO:
        //needs a rework - there's some weird ghost shit going on with the center of the map
        
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

            // if (nonProceduralArea.prefab != null)
            // {
            //     Instantiate(nonProceduralArea.prefab,nonProceduralArea.spawnPosition,quaternion.identity,grid.transform);
            //     tilemapList.Add(nonProceduralArea.prefab.GetComponent<Tilemap>());
            // }
        }

        spawnedNonProceduralAreas = true;
        ScanAllTiles();
        //GenerateGridNodes();
    }
    
    

    private void GenerateBorderNodes()
    {
        //left and right borders
        for (int y = -levelHeight / 2; y <= levelHeight / 2; y++)
        {
            Vector3Int posLeft = new Vector3Int((-levelWidth/2), y, 0);
            gridNodeReferences[posLeft.x+levelWidth/2, posLeft.y+levelHeight/2].borderNode = true;
            
            Vector3Int posRight = new Vector3Int((levelWidth/2), y, 0);
            gridNodeReferences[posRight.x+levelWidth/2, posRight.y+levelHeight/2].borderNode = true;
        }
        
        //for top and bottom borders
        for (int x = -levelWidth / 2; x <= levelWidth / 2; x++)
        {
            Vector3Int posBottom = new Vector3Int(x, (-levelHeight/2), 0);
            gridNodeReferences[posBottom.x+levelWidth/2, posBottom.y+levelHeight/2].borderNode = true;
            
            Vector3Int posTop = new Vector3Int(x, (levelHeight/2), 0);
            gridNodeReferences[posTop.x+levelWidth/2, posTop.y+levelHeight/2].borderNode = true;
        }
        borderNodesGenerated = true;
    }

    private void ClearTiles()
    {
        borderGenerated = false;
        spawnedNonProceduralAreas = false;
        currentTilemap.ClearAllTiles();
        blockedNodes.Clear();
        fullNeighbours.Clear();
        reachableTiles.Clear();
        optimalPath = null;
        secondaryPath = null;
        floodingStarted = false;
        foreach (Transform child in grid.transform)
        {
            child.GetComponent<Tilemap>().ClearAllTiles();
            if (child != currentTilemap.transform) Destroy(child.transform.gameObject);
        }

        GenerateTiles();
        //clearTilesEvent?.Invoke();
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
