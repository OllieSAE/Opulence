using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public Tilemap tilemap;
    public List<CustomTile> tiles = new List<CustomTile>();

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
        {
            SaveLevel();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
        {
            LoadLevel();
        }
    }

    void SaveLevel()
    {
        BoundsInt bounds = tilemap.cellBounds;

        LevelData levelData = new LevelData();

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                TileBase temp = tilemap.GetTile(new Vector3Int(x, y, 0));

                CustomTile tempTile = tiles.Find(t => t.tile == temp);
                
                if (tempTile != null)
                {
                    levelData.tiles.Add(tempTile.id);
                    levelData.positions_x.Add(x);
                    levelData.positions_y.Add(y);
                }
            }
        }

        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(Application.dataPath + "/levelGenTest.json", json);
    }

    void LoadLevel()
    {
        string json = File.ReadAllText(Application.dataPath + "/levelGenTest.json");
        LevelData data = JsonUtility.FromJson<LevelData>(json);
        
        tilemap.ClearAllTiles();

        for (int i = 0; i < data.tiles.Count; i++)
        {
            tilemap.SetTile(new Vector3Int(data.positions_x[i],data.positions_y[i],0), tiles.Find(t => t.name == data.tiles[i]).tile);
        }
    }

    public class LevelData
    {
        public List<string> tiles = new List<string>();
        public List<int> positions_x = new List<int>();
        public List<int> positions_y = new List<int>();
    }
}
