using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Astar : MonoBehaviour
{
    public static Astar Instance;
    public int skipTime = 100;
    public int breakTime = 1000;
    public float heightTollerence = 1; // Used for the height in path simplifying using

    private void Awake()
    {
        Instance = this;
    }


    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        float maxIncline = 9; // TODO

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node start_node = AstarGrid.Instance.NodeFromWorldPoint(request.pathStart);
        Node target_node = AstarGrid.Instance.NodeFromWorldPoint(request.pathEnd);

        Vector3[] waypoints = new Vector3[0];
        bool success = false;


        if (target_node.walkable || start_node.walkable)
        {
            Heap<Node> openList = new Heap<Node>(AstarGrid.Instance.MaxSize);
            List<Node> closedList = new List<Node>();
            openList.Add(start_node);

            while (openList.Count > 0)
            {
                Node currentNode = openList.RemoveFirst();

                closedList.Add(currentNode);

                if (currentNode == target_node)
                {
                    sw.Stop();
                    //UnityEngine.Debug.Log($"Path found in {sw.ElapsedMilliseconds}ms");
                    success = true;
                    break;
                }

                if (sw.ElapsedMilliseconds > breakTime)
                {
                    break;
                }

                foreach (Node neighbor in GetNeighbors(request.grid, currentNode))
                {
                    if (closedList.Contains(neighbor) || !neighbor.walkable)
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, target_node);
                        if (neighbor.position.y > currentNode.position.y)
                            neighbor.yCost = (int)(Mathf.Clamp(neighbor.position.y - currentNode.position.y, 0, 10) * 5);
                        else if (neighbor.position.y < currentNode.position.y)
                            neighbor.yCost = (int)(Mathf.Clamp(currentNode.position.y - neighbor.position.y, 0, 10) * 5);
                        else
                            neighbor.yCost = 0;
                        neighbor.parent = currentNode;

                        if (!neighbor.walkable)
                            currentNode.wCost += 2;

                        if (!openList.Contains(neighbor) && neighbor.yCost < maxIncline)
                            openList.Add(neighbor);
                        else
                            openList.UpdateItem(neighbor);

                    }
                }
            }
        }

        if (success)
        {
            waypoints = TracePath(start_node, target_node);
            success = waypoints.Length > 0;
        }
        else
        {
            sw.Stop();
            UnityEngine.Debug.Log($"No path found after {sw.ElapsedMilliseconds}ms");
        }
        callback(new PathResult(waypoints, success, request.callback));
    }

    Vector3[] TracePath(Node startNode, Node endNode) 
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].position);
            }
            else
            {
                if (waypoints.Count > 0)
                {
                    if (waypoints[waypoints.Count-1].y - path[i].position.y > heightTollerence || waypoints[waypoints.Count - 1].y - path[i].position.y < -heightTollerence)
                    {
                        waypoints.Add(path[i].position);
                    }
                }
            }
            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    List<Node> GetNeighbors(Node[,] grid, Node node)
    {
        List<Node> neighbors = new List<Node>();
        int xPos = node.gridX;
        int yPos = node.gridY;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                if (x+xPos < grid.GetLength(0) && y+yPos < grid.GetLength(0) && x+xPos > -1 && y+yPos > -1) { neighbors.Add(grid[x+xPos,y+yPos]); }
            }
        }

        return neighbors;
    }

    int GetDistance(Node orgin, Node target)
    {
        int xDist = Mathf.Abs(orgin.gridX - target.gridX);
        int yDist = Mathf.Abs(orgin.gridY - target.gridY);

        if (xDist > yDist)
            return 14 * yDist + 10 * (xDist - yDist);
        return 14 * xDist + 10 * (yDist - xDist);
    }
}
