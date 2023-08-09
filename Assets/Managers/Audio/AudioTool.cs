using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;


public class AudioTool : MonoBehaviour
{
    private PolygonCollider2D collider;
    
    //would need to update this if Odin plugin is no longer being used
    //probably just Header
    //or learn to make a custom inspector
    [FoldoutGroup("Polygon Collider 2D Settings")] public PhysicsMaterial2D material;
    [FoldoutGroup("Polygon Collider 2D Settings")] public bool setColliderToTrigger = true;
    [FoldoutGroup("Polygon Collider 2D Settings")] public bool usedByEffector = false;
    [FoldoutGroup("Polygon Collider 2D Settings")] public bool usedByComposite = false;
    [FoldoutGroup("Polygon Collider 2D Settings")] public bool autoTiling = false;
    [FoldoutGroup("Polygon Collider 2D Settings")] public float offsetX = 0;
    [FoldoutGroup("Polygon Collider 2D Settings")] public float offsetY = 0;

    [Header ("User Settings")]
    public bool showLine = true;
    public Color lineColor = Color.red;
    
    [Header ("Note - adjusting variables in playmode won't save them! Screenshot!")]
    public Vector2[] points = new Vector2[]
    {
        //manually set the values of a default PolygonCollider2D
        //this removes the finnicky-ness of generating the points from five (0,0) vector2s
        //much more user-friendly!
        new Vector2(0f,1f),
        new Vector2(-0.9510565f,0.309017f),
        new Vector2(-0.5877852f,-0.8090171f),
        new Vector2(0.5877854f,-0.8090169f),
        new Vector2(0.9510565f,0.3090171f)
    };

    private void Start()
    {
        if (collider == null)
        {
            GeneratePolygonCollider2D();
        }

        //auto disables line on entering playmode
        showLine = false;
    }

    private void GeneratePolygonCollider2D()
    {
        //create and assign the collider
        collider = gameObject.AddComponent<PolygonCollider2D>();
        
        //define/set collider parameters
        //may need to expand on this if needed
        if (material != null) collider.sharedMaterial = material;
        collider.isTrigger = setColliderToTrigger;
        collider.usedByEffector = usedByEffector;
        collider.usedByComposite = usedByComposite;
        collider.autoTiling = autoTiling;
        collider.offset = new Vector2(offsetX, offsetY);
        
        //assigns collider points to chosen points
        collider.points = points;
    }

    public void OnDrawGizmos()
    {
        if (!showLine) return;
        
        //display in Object's local space rather than world coords
        Gizmos.matrix = transform.localToWorldMatrix;
        
        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine (new Vector3 (points [i].x, points [i].y), new Vector3 (points [i + 1].x, points [i + 1].y));
        }
        //for loop can't access and draw from the last point back to the first, so this is outside the loop
        Gizmos.DrawLine(new Vector3 (points [points.Length - 1].x, points[points.Length - 1].y), new Vector3 (points [0].x, points [0].y));
    }
}
