using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CinemachineConfiner : MonoBehaviour
{
    public Collider2D tilemapBoundary;
    public PolygonCollider2D confinerBoundary;
    public float offsetX;
    public float offsetY;

    private void Start()
    {
        GameManager.Instance.onLevelLoadedEvent += OnLevelLoaded;
    }

    private void OnDisable()
    {
        GameManager.Instance.onLevelLoadedEvent -= OnLevelLoaded;
    }

    private void OnLevelLoaded()
    {
        tilemapBoundary = GameObject.FindGameObjectWithTag("Tilemap Collider").GetComponent<CompositeCollider2D>();
        confinerBoundary = GetComponent<PolygonCollider2D>();
        SetConfinerBounds();
    }

    //NOTE: If you think this has stopped working - CHECK PARENT OBJECT TRANSFORM
    private void SetConfinerBounds()
    {
        Vector2[] points = confinerBoundary.points;
        
        //top middle
        points[0] = new Vector2(tilemapBoundary.bounds.center.x - offsetX, tilemapBoundary.bounds.center.y + tilemapBoundary.bounds.extents.y - offsetY);
        
        //top left
        points[1] = new Vector2(tilemapBoundary.bounds.center.x - tilemapBoundary.bounds.extents.x + offsetX, tilemapBoundary.bounds.center.y + tilemapBoundary.bounds.extents.y - offsetY);
        
        //bottom left
        points[2] = new Vector2(tilemapBoundary.bounds.center.x - tilemapBoundary.bounds.extents.x + offsetX, tilemapBoundary.bounds.center.y - tilemapBoundary.bounds.extents.y + offsetY);
        
        //bottom right
        points[3] = new Vector2(tilemapBoundary.bounds.center.x + tilemapBoundary.bounds.extents.x - offsetX, tilemapBoundary.bounds.center.y - tilemapBoundary.bounds.extents.y + offsetY);
        
        //top right
        points[4] = new Vector2(tilemapBoundary.bounds.center.x + tilemapBoundary.bounds.extents.x - offsetX, tilemapBoundary.bounds.center.y + tilemapBoundary.bounds.extents.y - offsetY);

        confinerBoundary.points = points;
    }
}
