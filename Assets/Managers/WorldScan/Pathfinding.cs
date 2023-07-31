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
    public Transform start2;
    public Transform end2;
    public bool showPath = false;
    private Heap<Node> openSet;
    private HashSet<Node> closedSet = new HashSet<Node>();
    private Node lowestHCost;
    public short playerJumpValue;
    public int pathAttempts = 0;
    
    public delegate void RestartLevelGenEvent();
    public event RestartLevelGenEvent restartLevelGenEvent;

    public delegate void LevelGenSuccessEvent();
    public event LevelGenSuccessEvent levelGenSuccessEvent;
    
    private void Awake()
    {
        levelGenerator = GetComponent<LevelGenerator>();
    }

    private void Start()
    {
        levelGenerator.clearTilesEvent += ClearGizmosLists;
    }

    private void OnDisable()
    {
        levelGenerator.clearTilesEvent -= ClearGizmosLists;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Jump"))FindPath(start2.position, end2.position, playerJumpValue);
    }

    public void FindDefaultPath()
    {
        FindPath(start.position, end.position, playerJumpValue);
    }

    void FindPath(Vector2 startPos, Vector2 targetPos, short maxPlayerJump)
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
        startNode.jumpValue = 0;

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in levelGenerator.GetNeighbours(currentNode))
            {
                if (neighbour != null)
                {
                    if (neighbour.isTile || closedSet.Contains(neighbour)) continue;

                    if (!neighbour.isReachable) continue;
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
        
        
        //this needs a failsafe if it CANNOT find a successful path
        StartCoroutine(WaitForPathRetry(startPos, targetPos, maxPlayerJump));

    }

    private IEnumerator WaitForPathRetry(Vector2 startPos, Vector2 targetPos, short playerJump)
    {
        pathAttempts++;
        yield return new WaitForSeconds(0.01f);
        if (pathAttempts < 50)
        {
            FindPath(startPos, targetPos, playerJump);
        }
        else
        {
            pathAttempts = 0;
            restartLevelGenEvent?.Invoke();
        }
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

    private void ClearGizmosLists()
    {
        lowestHCost = null;
        pathAttempts = 0;
        if(levelGenerator.path!=null) levelGenerator.path.Clear();
        closedSet.Clear();
        openSet.Clear();
        StopAllCoroutines();
        StartCoroutine(PathFoundRestartTest());
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

        print("path successful");
        levelGenSuccessEvent?.Invoke();
        
        //StartCoroutine(PathFoundRestartTest());
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

    private IEnumerator PathFoundRestartTest()
    {
        pathAttempts = 0;
        yield return new WaitForSeconds(0.1f);
        restartLevelGenEvent?.Invoke();
    }
}
