using System.Collections.Generic;
using UnityEngine;

public class AstarGrid : MonoBehaviour
{
    public static AstarGrid Instance;
    public Vector3 positionOffset;
    public LayerMask terrainMask;
    public LayerMask obsticleMask;
    public int gridSize = 100;
    public int sampleSize = 50;
    public float colliderRadius = 0.5f;
    public float walkableUpdateTime = 0.5f;
    public float gridUpdateTime = 1f;

    public Node[,] nodeGrid;
    [Space]
    public Transform target;
    [Space]
    public bool debugView = false;

    public void Awake()
    {
        Instance = this;
    }

    public int MaxSize
    {
        get
        {
            return sampleSize * sampleSize;
        }
    }

    float timer = 9999;
    float timer2 = 9999;
    float timer3 = 9999;
    public void Update()
    {
        timer += Time.deltaTime;
        timer2 += Time.deltaTime;
        timer3 += Time.deltaTime;
        if (timer >= walkableUpdateTime && nodeGrid != null)
        {
            timer = 0;
            foreach (Node node in nodeGrid)
            {
                if (node.grounded)
                    node.walkable = !Physics.CheckSphere(node.position, colliderRadius, obsticleMask);
                else
                    node.walkable = false;
            }
        }
        if (timer2 >= gridUpdateTime)
        {
            nodeGrid = SetNodes(gridSize, sampleSize);
            timer = 9999;
            timer2 = 0;
        }
    }

    int _samples = 0;
    public Node[,] SetNodes(int size, int samples)
    {
        if (_samples != samples || nodeGrid == null)
            nodeGrid = new Node[samples, samples];

        for (int x = 0; x < samples; x++)
        {
            for (int y = 0; y < samples; y++)
            {
                nodeGrid[x, y] = new Node(x, y);
                nodeGrid[x, y].position = new Vector3(
                    (transform.position.x + positionOffset.x + ((size / samples) * x)),
                    transform.position.y + positionOffset.y,
                    (transform.position.z + positionOffset.z + ((size / samples) * y))
                );
                RaycastHit hit;
                if (Physics.Raycast(nodeGrid[x, y].position, Vector3.down, out hit, positionOffset.y + 50, terrainMask))
                {
                    nodeGrid[x, y].position.y = hit.point.y;
                    nodeGrid[x, y].grounded = true;
                }
            }
        }

        return nodeGrid;
    }

    private void OnDrawGizmos()
    {
        if (debugView && nodeGrid != null)
        {
            foreach (Node node in nodeGrid)
            {
                if (!node.grounded) continue;

                if (node.walkable)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawCube(node.position, Vector3.one * colliderRadius);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 position)
    {
        if (nodeGrid == null) return null;
        Node currentNode = nodeGrid[0,0];
        foreach (Node node in nodeGrid)
        {
            if (Vector3.Distance(node.position, position) < Vector3.Distance(currentNode.position, position))
            {
                currentNode = node;
            }
        }

        return currentNode;
    }
}
