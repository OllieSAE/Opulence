using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject meleeEnemy, rangedEnemy, chargerEnemy;
    public List<Node> enemyNodes = new List<Node>();
    
    public void SpawnEnemies()
    {
        while (enemyNodes.Count > 0)
        {
            List<Node> nodesToCheck = new List<Node>(enemyNodes);
            foreach (Node node in nodesToCheck)
            {
                Instantiate(meleeEnemy, node.gridPositionGizmosOnly + Vector3.up, Quaternion.identity);
                enemyNodes.Remove(node);
            }
        }
    }
}
