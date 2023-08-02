using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private CustomTile currentTile;
    [SerializeField] private List<CustomTile> customTiles;
    [SerializeField] private Grid grid;
    public GameObject player;
    private Pathfinding pathfinding;
    public bool gizmosOn;
    [Header("Tile Types")] private CustomTile enemyTile, spikeTile, ruleTile, collectibleTile;

    private Vector3Int[,] tileArrayVector3Ints;
    private CustomTile[,] tile2DArray;
    private Node[,] gridNodeReferences;
    private List<Node> blockedNodes;
    private List<Node> fullNeighbours = new List<Node>();
    private List<Node> outliers = new List<Node>();
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
    
    public delegate void ClearTilesEvent();
    public event ClearTilesEvent clearTilesEvent;

    private void Awake()
    {
        tileArrayVector3Ints = new Vector3Int[levelHeight, levelWidth];
        tile2DArray = new CustomTile[levelHeight, levelWidth];
        
        blockedNodes = new List<Node>();
        scale = Random.Range(0.15f, 0.25f);
        pathfinding = GetComponent<Pathfinding>();
    }

    public int MaxSize
    {
        get
        {
            return levelHeight * levelWidth;
        }
    }

    private void Update()
    { 
        
        //if(optimalPath!= null) print(optimalPath.Count);
        scale = Random.Range(0.15f, 0.25f);
        ScanAllTiles();
        //or change the 10000 
        
        Vector3Int pos = currentTilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

        //ClearTiles();
        //GenerateTiles();

        if (Input.GetMouseButtonDown(0))
        {
            //GetTileInfo(pos);
        }
        
        if (Input.GetMouseButtonDown(2))
        {
            //GenerateTiles();
            //PlaceTile(pos);
        }

        if (Input.GetMouseButton(1))
        {
            //ClearTiles();
            //DeleteTile(pos);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            //ClearSingleOutliers();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            //FillIndividualEmpty();
        }
    }

    public void GetTileInfo(Vector3Int location)
    {
        
        Node node = gridNodeReferences[location.x+levelWidth/2, location.y+levelHeight/2];
        

    }

    private void Start()
    {
        if (cam == null) cam = FindObjectOfType<Camera>();
        spikeTile = customTiles.Find(t => t.tileType == CustomTile.TileType.spike);
        enemyTile = customTiles.Find(t => t.tileType == CustomTile.TileType.enemy);
        tilemapList.Add(currentTilemap);
        pathfinding.restartLevelGenEvent += ClearTiles;
        pathfinding.levelGenSuccessEvent += LevelGenCompleteToGameManager;
        GenerateTiles();
    }

    private void OnDisable()
    {
        pathfinding.restartLevelGenEvent -= ClearTiles;
        pathfinding.levelGenSuccessEvent -= LevelGenCompleteToGameManager;
    }

    private void LevelGenCompleteToGameManager()
    {
        BlockInPath();
        //GameManager.Instance.LevelGenComplete();
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
                    currentTilemap.SetTile(optimalPath[i].neighbours[1,0].gridPosV3Int,currentTile.tile);
                }
                
                //if next step of path is to my LEFT, and previous is NOT BELOW current, put tile below current
                if (optimalPath[i + 1] == optimalPath[i].neighbours[0, 1] && optimalPath[i-1] != optimalPath[i].neighbours[1,0])
                {
                    currentTilemap.SetTile(optimalPath[i].neighbours[1,0].gridPosV3Int,currentTile.tile);
                }

                //if next step of path is ABOVE && previous step is BELOW
                if (optimalPath[i + 1] == optimalPath[i].neighbours[1, 2] && optimalPath[i-1] == optimalPath[i].neighbours[1,0])
                {
                    //if no tile to left, add tile to right
                    if(optimalPath[i].neighbours[0,1].isTile == false) currentTilemap.SetTile(optimalPath[i].neighbours[2,1].gridPosV3Int,currentTile.tile);
                    //if no tile to right, add tile to left
                    else if (optimalPath[i].neighbours[2, 1].isTile == false)
                        currentTilemap.SetTile(optimalPath[i].neighbours[0, 1].gridPosV3Int, currentTile.tile);
                }
                
                //if next step is ABOVE and previous step is LEFT or RIGHT
                if(optimalPath[i+1] == optimalPath[i].neighbours[1,2] && (optimalPath[i-1] == optimalPath[i].neighbours[0,1] || optimalPath[i-1] == optimalPath[i].neighbours[2,1]))
                {
                    //if no tile below, set tile below
                    if(optimalPath[i].neighbours[1,0].isTile == false) currentTilemap.SetTile(optimalPath[i].neighbours[1,0].gridPosV3Int,currentTile.tile);
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
                    if(!optimalPath.Contains(secondaryPath[i].neighbours[1,0])) currentTilemap.SetTile(secondaryPath[i].neighbours[1,0].gridPosV3Int,currentTile.tile);
                }
                
                //if next step of path is to my LEFT, and previous is NOT BELOW current, put tile below current
                if (secondaryPath[i + 1] == secondaryPath[i].neighbours[0, 1] && secondaryPath[i-1] != secondaryPath[i].neighbours[1,0])
                {
                    if(!optimalPath.Contains(secondaryPath[i].neighbours[1,0])) currentTilemap.SetTile(secondaryPath[i].neighbours[1,0].gridPosV3Int,currentTile.tile);
                }

                //if next step of path is ABOVE && previous step is BELOW
                if (secondaryPath[i + 1] == secondaryPath[i].neighbours[1, 2] && secondaryPath[i-1] == secondaryPath[i].neighbours[1,0])
                {
                    //if no tile to right, add tile to left (as long as it's not in optimal path)
                    if (secondaryPath[i].neighbours[2, 1].isTile == false)
                    {
                        if(!optimalPath.Contains(secondaryPath[i].neighbours[2,1])) currentTilemap.SetTile(secondaryPath[i].neighbours[2,1].gridPosV3Int,currentTile.tile);
                    }
                    //if no tile to left, add tile to right (as long as it's not in optimal path)
                    else if (secondaryPath[i].neighbours[0, 1].isTile == false)
                    {
                        if(!optimalPath.Contains(secondaryPath[i].neighbours[0,1])) currentTilemap.SetTile(secondaryPath[i].neighbours[0, 1].gridPosV3Int, currentTile.tile);
                    }
                }
                
                //if next step is ABOVE and previous step is LEFT or RIGHT
                if(secondaryPath[i+1] == secondaryPath[i].neighbours[1,2] && (secondaryPath[i-1] == secondaryPath[i].neighbours[0,1] || secondaryPath[i-1] == secondaryPath[i].neighbours[2,1]))
                {
                    //if no tile below, set tile below
                    if(secondaryPath[i].neighbours[1,0].isTile == false && !optimalPath.Contains(secondaryPath[i].neighbours[1,0])) currentTilemap.SetTile(secondaryPath[i].neighbours[1,0].gridPosV3Int,currentTile.tile);
                }
            }
        }
        
    }

    public List<Node> path;
    public List<Node> optimalPath;
    public List<Node> secondaryPath;
    private void OnDrawGizmos()
    {
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

    private void GenerateTiles()
    {
        for (int x = -levelWidth/2; x < levelWidth/2; x++)
        {
            for (int y = -levelHeight/2; y < levelHeight/2; y++)
            {
                //DELIBERATELY have the XY back to front so it draws horizontally first
                Vector3Int pos = new Vector3Int(x, y, 0);
                
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
        gridNodeReferences = null;
        gridNodeReferences = new Node[levelHeight+1, levelWidth+1];
        for (int x = (-levelWidth / 2); x < (levelWidth / 2) + 1; x++)
        {
            for (int y = (-levelHeight / 2); y < (levelHeight / 2) + 1; y++)
            {
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2] = new Node();
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].xPosInArray = x + levelWidth/2;
                gridNodeReferences[x + levelWidth/2, y + levelHeight/2].yPosInArray = y + levelHeight/2;
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

                //need to tweak wall jump and then revisit how high you can actually jump
                if (node.neighbours[0, 1] != null && node.neighbours[0, 1].isTile) node.isReachable = true;
                if (node.neighbours[2, 1] != null && node.neighbours[2, 1].isTile) node.isReachable = true;

                //one jump right
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
                
                //two jump right
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
                
                //three jump right
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
                
                //one jump left
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
                
                //two jump left
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
                
                //three jump left
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
                //left edge does not get [2,1] neighbour set because that is directly left of center
                if (x > floorX) gridNodeReferences[x + offsetX-1, y + offsetY].neighbours[2,1] = gridNodeReferences[x + offsetX, y + offsetY];

                //bottom edge 
                if (y > floorY) gridNodeReferences[x + offsetX, y + offsetY-1].neighbours[1,2] = gridNodeReferences[x + offsetX, y + offsetY];

                //right edge
                if (x < sizeX-1) gridNodeReferences[x + offsetX+1, y + offsetY].neighbours[0,1] = gridNodeReferences[x + offsetX, y + offsetY];
                
                //top edge
                if (y < sizeY-1) gridNodeReferences[x + offsetX, y + offsetY+1].neighbours[1,0] = gridNodeReferences[x + offsetX, y + offsetY];
                
                //bottom left corner
                if (x > floorX && y > floorY) gridNodeReferences[x + offsetX-1, y + offsetY-1].neighbours[2,2] = gridNodeReferences[x + offsetX, y + offsetY];
                
                //top left corner
                if (x > floorX && y < sizeY-1) gridNodeReferences[x + offsetX-1, y + offsetY+1].neighbours[2,0] = gridNodeReferences[x + offsetX, y + offsetY];
                
                //bottom right corner
                if (x < sizeX-1 && y > floorY) gridNodeReferences[x + offsetX+1, y + offsetY-1].neighbours[0,2] = gridNodeReferences[x + offsetX, y + offsetY];
                
                //top right corner
                if (x < sizeX-1 && y < sizeY-1) gridNodeReferences[x + offsetX+1, y + offsetY+1].neighbours[0,0] = gridNodeReferences[x + offsetX, y + offsetY];
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
            currentTilemap.SetTile(node.gridPosV3Int,currentTile.tile);
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
        print("map finished");
        yield return new WaitForSeconds(1f);
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
        optimalPath = null;
        secondaryPath = null;
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
        currentTilemap.SetTile(pos, currentTile.tile);
    }

    private void DeleteTile(Vector3Int pos)
    {
        currentTilemap.SetTile(pos, null);
    }
}
