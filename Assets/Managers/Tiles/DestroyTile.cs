using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestroyTile : MonoBehaviour
{
    private Tilemap tilemap;
    private Vector3Int myPos;
    private bool exploded = false;

    private void Start()
    {
        tilemap = FindObjectOfType<Tilemap>();
        myPos = new Vector3Int((int)(transform.position.x - 0.5f), (int)transform.position.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("TileBreaker"))
        {
            tilemap.SetTile(myPos, null);
            if (!exploded)
            {
                exploded = true;
                //play exploded sound
                //play exploded vfx
            }
        }
    }
}
