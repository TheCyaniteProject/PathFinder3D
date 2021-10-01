using UnityEngine;

public class Node
{
    public Node parent;
    public Vector3 position;
    public int gridX;
    public int gridY;
    public bool walkable = true;
    public bool grounded = false;

    public int gCost = 0; // cost to next node
    public int hCost = 0; // cost to reach target
    public int wCost = 0; // additinal weighted cost (from terrain type etc)
    public int yCost = 0; // additinal cost from world height
    public int fCost { get { return gCost + hCost + wCost + yCost; } }

    public Node(int x, int y)
    {
        gridX = x;
        gridY = y;
    }
}
