using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{
    public static List<Node> FindPath(Node[,] grid, Vector3 start, Vector3 target)
    {
        Node start_node = grid[0,0];
        Node target_node = grid[0,0];

        // initialize
        foreach (Node node in grid)
        {
            if (Vector3.Distance(node.position, start) < Vector3.Distance(start_node.position, start))
            {
                start_node = node;
            }
            if (Vector3.Distance(node.position, target) < Vector3.Distance(target_node.position, target))
            {
                target_node = node;
            }
        }

        List<Node> openList = new List<Node> { start_node };
        List<Node> closedList = new List<Node>();

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == target_node)
            {
                return TracePath(start_node, target_node);
            }

            foreach (Node neighbor in GetNeighbors(grid, currentNode))
            {
                if (closedList.Contains(neighbor) || !neighbor.walkable)
                    continue;

                int newMovementCOstToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCOstToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCOstToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, target_node);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    static List<Node> TracePath(Node startNode, Node endNode) 
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        return path;
    }

    static List<Node> GetNeighbors(Node[,] grid, Node node)
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

    static int GetDistance(Node orgin, Node target)
    {
        int xDist = Mathf.Abs(orgin.gridX - target.gridX);
        int yDist = Mathf.Abs(orgin.gridY - target.gridY);

        if (xDist > yDist)
            return 14 * yDist + 10 * (xDist - yDist);
        return 14 * xDist + 10 * (yDist - xDist);
    }
}
