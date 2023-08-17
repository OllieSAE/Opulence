using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnexploredTileCollider : MonoBehaviour
{
    public Tilemap unexploredTilemap;
    public Vector3 offset;
    public Vector3Int myPos;

    private void Start()
    {
        unexploredTilemap = LevelGenerator.Instance.mapGenerator.unexploredMapTilemap;
        offset = LevelGenerator.Instance.mapGenerator.offset;
        myPos = new Vector3Int((int)(transform.position.x - 0.5f - (int)offset.x), (int)transform.position.y-(int)offset.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("TileBreaker"))
        {
            unexploredTilemap.SetTile(myPos,null);
            Destroy(this);
        }
    }
}
