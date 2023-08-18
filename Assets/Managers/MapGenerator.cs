using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap mapTilemap;
    public Tilemap unexploredMapTilemap;
    public RuleTile ruleTile;
    public TileBase unexploredTile;
    public Grid mapGrid;
    public GameObject unexploredMapColliderParent;
    public GameObject unexploredColliderPrefab;
    private GameObject player;
    public GameObject playerMapClonePrefab;
    private GameObject playerMapClone;
    public Vector3 offset;
    private int levelWidth;
    private int levelHeight;
    private Node[,] mapNodes;
    public float timer;
    public float mapUpdateTimer = 1f;
    private bool cloneFound = false;

    private void Start()
    {
        player = LevelGenerator.Instance.player;
        offset = mapGrid.transform.position;
        levelHeight = LevelGenerator.Instance.levelHeight;
        levelWidth = LevelGenerator.Instance.levelWidth;
        GameManager.Instance.onLevelLoadedEvent += OnLevelLoad;
    }
    
    private void OnDisable()
    {
        GameManager.Instance.onLevelLoadedEvent -= OnLevelLoad;
    }

    private void OnLevelLoad()
    {
        if (player != null)
        {
            playerMapClone = Instantiate(playerMapClonePrefab,player.transform.position,quaternion.identity);
            GameManager.Instance.playerMapClone = playerMapClone;
            cloneFound = true;
        }
    }

    private void Update()
    {
        if(cloneFound) playerMapClone.transform.position = player.transform.position + offset;
        timer += Time.deltaTime;
    }

    public void LevelGenComplete()
    {
        mapNodes = LevelGenerator.Instance.gridNodeReferences;
        GenerateBorder();
    }

    private void GenerateBorder()
    {
        //left and right borders + 1, 2 and 3 offset
        for (int y = -levelHeight / 2 - 8; y <= levelHeight / 2 + 8; y++)
        {
            Vector3Int posLeft = new Vector3Int((-levelWidth/2), y, 0);
            mapTilemap.SetTile(posLeft,ruleTile);
            Vector3Int posLeft1 = new Vector3Int((-levelWidth / 2 - 1), y, 0);
            mapTilemap.SetTile(posLeft1,ruleTile);
            Vector3Int posLeft2 = new Vector3Int((-levelWidth / 2 - 2), y, 0);
            mapTilemap.SetTile(posLeft2,ruleTile);
            Vector3Int posLeft3 = new Vector3Int((-levelWidth / 2 - 3), y, 0);
            mapTilemap.SetTile(posLeft3,ruleTile);
            Vector3Int posLeft4 = new Vector3Int((-levelWidth / 2 - 4), y, 0);
            mapTilemap.SetTile(posLeft4,ruleTile);
            Vector3Int posLeft5 = new Vector3Int((-levelWidth / 2 - 5), y, 0);
            mapTilemap.SetTile(posLeft5,ruleTile);
            Vector3Int posLeft6 = new Vector3Int((-levelWidth / 2 - 6), y, 0);
            mapTilemap.SetTile(posLeft6,ruleTile);
            Vector3Int posLeft7 = new Vector3Int((-levelWidth / 2 - 7), y, 0);
            mapTilemap.SetTile(posLeft7,ruleTile);
            Vector3Int posLeft8 = new Vector3Int((-levelWidth / 2 - 8), y, 0);
            mapTilemap.SetTile(posLeft8,ruleTile);
            Vector3Int posRight = new Vector3Int((levelWidth/2), y, 0);
            mapTilemap.SetTile(posRight,ruleTile);
            Vector3Int posRight1 = new Vector3Int((levelWidth/2 + 1), y, 0);
            mapTilemap.SetTile(posRight1,ruleTile);
            Vector3Int posRight2 = new Vector3Int((levelWidth/2 + 2), y, 0);
            mapTilemap.SetTile(posRight2,ruleTile);
            Vector3Int posRight3 = new Vector3Int((levelWidth/2 + 3), y, 0);
            mapTilemap.SetTile(posRight3,ruleTile);
            Vector3Int posRight4 = new Vector3Int((levelWidth/2 + 4), y, 0);
            mapTilemap.SetTile(posRight4,ruleTile);
            Vector3Int posRight5 = new Vector3Int((levelWidth/2 + 5), y, 0);
            mapTilemap.SetTile(posRight5,ruleTile);
            Vector3Int posRight6 = new Vector3Int((levelWidth/2 + 6), y, 0);
            mapTilemap.SetTile(posRight6,ruleTile);
            Vector3Int posRight7 = new Vector3Int((levelWidth/2 + 7), y, 0);
            mapTilemap.SetTile(posRight7,ruleTile);
            Vector3Int posRight8 = new Vector3Int((levelWidth/2 + 8), y, 0);
            mapTilemap.SetTile(posRight8,ruleTile);
        }
        
        //for top and bottom borders + 1, 2 and 3 offset
        for (int x = -levelWidth / 2 - 8; x <= levelWidth / 2 + 8; x++)
        {
            Vector3Int posBottom = new Vector3Int(x, (-levelHeight/2), 0);
            mapTilemap.SetTile(posBottom,ruleTile);
            Vector3Int posBottom1 = new Vector3Int(x, (-levelHeight/2 - 1), 0);
            mapTilemap.SetTile(posBottom1,ruleTile);
            Vector3Int posBottom2 = new Vector3Int(x, (-levelHeight/2 - 2), 0);
            mapTilemap.SetTile(posBottom2,ruleTile);
            Vector3Int posBottom3 = new Vector3Int(x, (-levelHeight/2 - 3), 0);
            mapTilemap.SetTile(posBottom3,ruleTile);
            Vector3Int posBottom4 = new Vector3Int(x, (-levelHeight/2 - 4), 0);
            mapTilemap.SetTile(posBottom4,ruleTile);
            Vector3Int posBottom5 = new Vector3Int(x, (-levelHeight/2 - 5), 0);
            mapTilemap.SetTile(posBottom5,ruleTile);
            Vector3Int posBottom6 = new Vector3Int(x, (-levelHeight/2 - 6), 0);
            mapTilemap.SetTile(posBottom6,ruleTile);
            Vector3Int posBottom7 = new Vector3Int(x, (-levelHeight/2 - 7), 0);
            mapTilemap.SetTile(posBottom7,ruleTile);
            Vector3Int posBottom8 = new Vector3Int(x, (-levelHeight/2 - 8), 0);
            mapTilemap.SetTile(posBottom8,ruleTile);
            Vector3Int posTop = new Vector3Int(x, (levelHeight/2), 0);
            mapTilemap.SetTile(posTop,ruleTile);
            Vector3Int posTop1 = new Vector3Int(x, (levelHeight/2 + 1), 0);
            mapTilemap.SetTile(posTop1,ruleTile);
            Vector3Int posTop2 = new Vector3Int(x, (levelHeight/2 + 2), 0);
            mapTilemap.SetTile(posTop2,ruleTile);
            Vector3Int posTop3 = new Vector3Int(x, (levelHeight/2 + 3), 0);
            mapTilemap.SetTile(posTop3,ruleTile);
            Vector3Int posTop4 = new Vector3Int(x, (levelHeight/2 + 4), 0);
            mapTilemap.SetTile(posTop4,ruleTile);
            Vector3Int posTop5 = new Vector3Int(x, (levelHeight/2 + 5), 0);
            mapTilemap.SetTile(posTop5,ruleTile);
            Vector3Int posTop6 = new Vector3Int(x, (levelHeight/2 + 6), 0);
            mapTilemap.SetTile(posTop6,ruleTile);
            Vector3Int posTop7 = new Vector3Int(x, (levelHeight/2 + 7), 0);
            mapTilemap.SetTile(posTop7,ruleTile);
            Vector3Int posTop8 = new Vector3Int(x, (levelHeight/2 + 8), 0);
            mapTilemap.SetTile(posTop8,ruleTile);
        }
        SetStartAndEndAreas();
    }

    private void SetStartAndEndAreas()
    {
        foreach (NonProceduralArea nonProceduralArea in LevelGenerator.Instance.nonProceduralAreas)
        {
            int tempSpawnPosX = (int)nonProceduralArea.spawnPosition.x;
            int tempSpawnPosY = (int)nonProceduralArea.spawnPosition.y;
            for (int x = -nonProceduralArea.width / 2; x < nonProceduralArea.width / 2; x++)
            {
                for (int y = -nonProceduralArea.height / 2; y < nonProceduralArea.height / 2; y++)
                {
                    Vector3Int pos = new Vector3Int(x+tempSpawnPosX, y+tempSpawnPosY, 0);
                    mapTilemap.SetTile(pos,null);
                }
            }
        }
        CopyLevelTiles();
    }

    private void CopyLevelTiles()
    {
        foreach (Node node in mapNodes)
        {
            if(node.isTile) mapTilemap.SetTile(node.gridPosV3Int,ruleTile);
            unexploredMapTilemap.SetTile(node.gridPosV3Int,unexploredTile);
            Instantiate(unexploredColliderPrefab, node.GridPosVector3 + offset, quaternion.identity, unexploredMapColliderParent.transform);
        }
    }


    public void UncoverExploredArea()
    {
        if (timer > mapUpdateTimer)
        {
            timer = 0;
            
            
        }
    }
}
