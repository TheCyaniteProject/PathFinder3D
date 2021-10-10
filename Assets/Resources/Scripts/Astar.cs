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
    public float minHeightDifference = 1; // Used for the height in path simplifying

    private void Awake()
    {
        Instance = this;
    }

    long stopWatch = 0;
    public IEnumerator FindPath(PathRequest request, Action<PathResult> callback)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node[,] grid = request.grid.nodeGrid;
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
                    success = true;
                    break;
                }

                if (sw.ElapsedMilliseconds - stopWatch > skipTime)
                {
                    stopWatch = sw.ElapsedMilliseconds;
                    UnityEngine.Debug.Log(true);
                    yield return null;
                }

                if (sw.ElapsedMilliseconds > breakTime)
                {
                    break;
                }

                foreach (Node neighbor in GetNeighbors(grid, currentNode))
                {
                    if (closedList.Contains(neighbor) || !neighbor.walkable)
                        continue;

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, target_node);

                        // Height costs
                        if (neighbor.position.y > currentNode.position.y)
                            neighbor.yCost = (int)(Mathf.Clamp(neighbor.position.y - currentNode.position.y, 0, 10) * 5);
                        else if (neighbor.position.y < currentNode.position.y)
                            neighbor.yCost = (int)(Mathf.Clamp(currentNode.position.y - neighbor.position.y, 0, 10) * 4);
                        else
                            neighbor.yCost = 0;

                        neighbor.parent = currentNode;

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                        else
                            openList.UpdateItem(neighbor);

                    }
                }
            }
        }

        yield return null;

        sw.Stop();
        if (success)
        {
            waypoints = TracePath(start_node, target_node);
            success = waypoints.Length > 0;
        }
        else
        {
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
                waypoints.Add(path[i -1].position);
            }
            else
            {
                if (waypoints.Count > 0)
                {
                    if (path[i - 1].position.y - path[i].position.y > minHeightDifference || path[i - 1].position.y - path[i].position.y < -minHeightDifference)
                    {
                        waypoints.Add(path[i].position);
                    }
                }
            }
            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    public static List<Node> GetNeighbors(Node[,] grid, Node node)
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
    public static List<Node> GetDirectNeighbors(Node[,] grid, Node node)
    {
        List<Node> neighbors = new List<Node>();
        int xPos = node.gridX;
        int yPos = node.gridY;

        if (xPos + 1 < AstarGrid.Instance.sampleSize && yPos + 1 < AstarGrid.Instance.sampleSize)
            neighbors.Add(grid[xPos + 1, yPos + 1]);
        if (0 <= xPos - 1 && 0 <= yPos - 1)
            neighbors.Add(grid[xPos - 1, yPos - 1]);
        if (xPos + 1 < AstarGrid.Instance.sampleSize && yPos - 1 >= 0)
            neighbors.Add(grid[xPos + 1, yPos - 1]);
        if (0 <= xPos - 1 && yPos + 1 < AstarGrid.Instance.sampleSize)
            neighbors.Add(grid[xPos - 1, yPos + 1]);

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
