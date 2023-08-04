using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyList = new List<GameObject>();
    public List<Node> enemyNodes = new List<Node>();
    
    public void SpawnEnemies()
    {
        while (enemyNodes.Count > 0)
        {
            List<Node> nodesToCheck = new List<Node>(enemyNodes);
            foreach (Node node in nodesToCheck)
            {
                Instantiate(enemyList[Random.Range(0,enemyList.Count)], node.gridPositionGizmosOnly + Vector3.up, Quaternion.identity);
                enemyNodes.Remove(node);
            }
        }
    }
}
