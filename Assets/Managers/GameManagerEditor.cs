using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
[CanEditMultipleObjects]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Toggle Enemy Movement"))
        {
            ((GameManager) target).ToggleEnemyMovement();
        }

        if (GUILayout.Button("Destroy First Floor"))
        {
            ((GameManager) target).DestroyFirstFloor();
        }
        
        if (GUILayout.Button("Destroy Second Floor"))
        {
            ((GameManager) target).DestroySecondFloor();
        }
    }
}
