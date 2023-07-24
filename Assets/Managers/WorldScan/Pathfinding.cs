using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class Pathfinding : MonoBehaviour
{
    private LevelGenerator levelGenerator;
    public Transform start;
    public Transform end;
    public bool showPath = false;
    private Heap<Node> openSet;
    private HashSet<Node> closedSet = new HashSet<Node>();
    private Node lowestHCost;
    private void Awake()
    {
        levelGenerator = GetComponent<LevelGenerator>();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Jump"))FindPath(start.position, end.position);
    }

    void FindPath(Vector2 startPos, Vector2 targetPos)
    {
         Node startNode = levelGenerator.NodeFromWorldPoint(startPos);
        Node targetNode = levelGenerator.NodeFromWorldPoint(targetPos);

        //TODO:
        //May need to consider optimizing heap further
        //RE: @mystman1210's comment on Lague's 3rd vid
        closedSet.Clear();
        if (openSet!=null) openSet.Clear();
        if (levelGenerator.path != null) levelGenerator.path.Clear();
        
        openSet = new Heap<Node>(levelGenerator.MaxSize);
        openSet.Add(startNode);
        startNode.hCost = GetDistance(startNode, targetNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode,targetNode);
                return;
            }

            foreach (Node neighbour in currentNode.neighbours)
            {
                if (neighbour != null)
                {
                    if (neighbour.isTile || closedSet.Contains(neighbour)) continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;
                    
                        if(!openSet.Contains(neighbour)) openSet.Add(neighbour);
                    }
                }
            }
        }

        int minH = closedSet.Min(node => node.hCost);
        lowestHCost = closedSet.FirstOrDefault(node => node.hCost == minH);
        levelGenerator.ClearAroundLowest(lowestHCost);
    }
    
    private void OnDrawGizmos()
    {
        if (closedSet != null)
        {
            foreach (Node node in closedSet)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(node.gridPositionGizmosOnly,Vector3.one);
            }
        }

        if (levelGenerator.path != null)
        {
            foreach (Node node in levelGenerator.path)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.gridPositionGizmosOnly,Vector3.one);
            }
        }

        if (lowestHCost != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(lowestHCost.gridPositionGizmosOnly,Vector3.one);
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        levelGenerator.path = path;
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        //  14y + 10 (x-y), where y is less than x
        //  14x + 10 (y-x), where x is less than y
        int distanceX = Mathf.Abs(nodeA.xPosInArray - nodeB.xPosInArray);
        int distanceY = Mathf.Abs(nodeA.yPosInArray - nodeB.yPosInArray);

        if (distanceX > distanceY) return 14 * distanceY + 10 * (distanceX - distanceY);
        return 14 * distanceX + 10 * (distanceY - distanceX);
    }
}
