using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private LevelGenerator levelGenerator;
    public Transform start;
    public Transform end;
    public bool showPath = false;

    private void Awake()
    {
        levelGenerator = GetComponent<LevelGenerator>();
    }

    private void Update()
    {
        if(showPath)FindPath(start.position, end.position);
    }

    void FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = levelGenerator.NodeFromWorldPoint(startPos);
        Node targetNode = levelGenerator.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if ((openSet[i].fCost < currentNode.fCost) || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
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
