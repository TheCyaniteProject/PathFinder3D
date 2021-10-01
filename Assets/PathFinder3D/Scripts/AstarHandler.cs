using System.Collections.Generic;
using UnityEngine;

public class AstarHandler : MonoBehaviour
{
    public Vector3 positionOffset;
    public int size = 100;
    public int samples = 50;
    public float colliderRadius = 0.5f;
    public float pathUpdateTime = 0.1f;
    public float gridUpdateTime = 0.2f;
    public LayerMask terrainMask;
    public LayerMask obsticleMask;

    public Node[,] nodeGrid;
    [Space]
    public Transform seeker;
    public Transform target;
    public List<List<Node>> paths = new List<List<Node>>();
    [Space]
    public bool debugView = false;

    public void Awake()
    {
        nodeGrid = GetNodes(size, samples);
    }

    float timer = 9999;
    float timer2 = 9999;
    public void Update()
    {
        timer += Time.deltaTime;
        timer2 += Time.deltaTime;
        if (timer >= pathUpdateTime)
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
            nodeGrid = GetNodes(size, samples);
            timer = 9999;
            timer2 = 0;
        }
    }

    public Node[,] GetNodes(int size, int samples)
    {
        nodeGrid = new Node[samples, samples];

        for (int x = 0; x < samples; x++)
        {
            for (int y = 0; y < samples; y++)
            {
                nodeGrid[x, y] = new Node(x, y);
                nodeGrid[x, y].position = new Vector3(
                    (transform.position.x + (size / samples * x)) + positionOffset.x,
                    transform.position.y + positionOffset.y,
                    (transform.position.z + (size / samples * y)) + positionOffset.z
                );
                RaycastHit hit;
                if (Physics.SphereCast(nodeGrid[x, y].position, colliderRadius, Vector3.down, out hit, positionOffset.y*2, terrainMask))
                {
                    nodeGrid[x, y].position.y = hit.point.y;
                    nodeGrid[x, y].yCost = (int)(nodeGrid[x, y].position.y * 10);
                    nodeGrid[x, y].grounded = true;
                }
            }
        }

        return nodeGrid;
    }

    private void OnDrawGizmos()
    {
        if (!debugView || nodeGrid == null) return;

        foreach (Node node in nodeGrid)
        {
            if (!node.grounded) continue;
            if (node.walkable)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.red;
            foreach (List<Node> path in paths)
            {
                if (path.Contains(node))
                    Gizmos.color = Color.yellow;
            }
            Gizmos.DrawSphere(node.position, colliderRadius);
        }
    }
}
