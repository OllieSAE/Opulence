using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCameraConfiner : MonoBehaviour
{
    public PolygonCollider2D tilemapCollider;
    public PolygonCollider2D confinerBoundary;
    public CinemachineVirtualCamera mapCamera;
    public CinemachineConfiner2D mapCameraConfiner;
    public float offsetX;
    public float offsetY;

    
    //NOTE THIS DOES NOT WORK AT ALL!!!!!!
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
        if (GameObject.FindGameObjectWithTag("Map Tilemap") != null)
        {
            tilemapCollider = GameObject.FindGameObjectWithTag("Map Tilemap").GetComponent<PolygonCollider2D>();
        }

        if (GameManager.Instance.playerMapClone != null)
        {
            mapCamera.m_Follow = GameManager.Instance.playerMapClone.transform;
        }

        if(tilemapCollider!=null) SetConfinerBounds();
    }
    
    private void SetConfinerBounds()
    {
        Vector2[] points = confinerBoundary.points;
        
        //top middle
        points[0] = new Vector2(tilemapCollider.bounds.center.x - offsetX, tilemapCollider.bounds.center.y + tilemapCollider.bounds.extents.y - offsetY);
        
        //top left
        points[1] = new Vector2(tilemapCollider.bounds.center.x - tilemapCollider.bounds.extents.x + offsetX, tilemapCollider.bounds.center.y + tilemapCollider.bounds.extents.y - offsetY);
        
        //bottom left
        points[2] = new Vector2(tilemapCollider.bounds.center.x - tilemapCollider.bounds.extents.x + offsetX, tilemapCollider.bounds.center.y - tilemapCollider.bounds.extents.y + offsetY);
        
        //bottom right
        points[3] = new Vector2(tilemapCollider.bounds.center.x + tilemapCollider.bounds.extents.x - offsetX, tilemapCollider.bounds.center.y - tilemapCollider.bounds.extents.y + offsetY);
        
        //top right
        points[4] = new Vector2(tilemapCollider.bounds.center.x + tilemapCollider.bounds.extents.x - offsetX, tilemapCollider.bounds.center.y + tilemapCollider.bounds.extents.y - offsetY);

        confinerBoundary.points = points;
        if(mapCameraConfiner!=null) StartCoroutine(InvalidateCache());
    }
    
    public IEnumerator InvalidateCache()
    {
        yield return new WaitForSeconds(1f);
        if (GameManager.Instance.playerMapClone != null)
        {
            mapCamera.m_Follow = GameManager.Instance.playerMapClone.transform;
        }
        mapCameraConfiner.InvalidateCache();
    }
}
