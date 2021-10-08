using UnityEngine;

[System.Serializable]
public class Node : IHeapItem<Node>
{
    public Node parent;
    public Vector3 position;
    public int gridX;
    public int gridY;
    public bool walkable = true; // if the spherecheck has not hit an obsticle
    public bool grounded = false; // if the raycast has hit terrain;

    public int gCost = 0; // cost to next node
    public int hCost = 0; // cost to reach target
    public int wCost = 0; // additinal weighted cost (from terrain type etc - NYI)
    public int yCost = 0; // additinal cost from world height
    public int fCost { get { return gCost + hCost + wCost + yCost; } }

    int heapIndex;

    public Node(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node node)
    {
        int compare = fCost.CompareTo(node.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(node.hCost);
        }
        return -compare;
    }
}
