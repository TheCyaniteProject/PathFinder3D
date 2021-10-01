using UnityEngine;
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    public AstarHandler astar;
    public float maxDistance = 1;
    public float speed = 1;
    List<Node> path = new List<Node>();

    Node closestNode;

    public void Update()
    {
        if (closestNode == null || Vector3.Distance(closestNode.position, transform.position) > maxDistance)
        {
            closestNode = null;

            foreach (Node node in astar.nodeGrid)
            {
                if (Vector3.Distance(node.position, transform.position) <= maxDistance)
                {
                    closestNode = node;
                    break;
                }
            }

            if (closestNode == null) return;
        }

        path.Clear();
        path.AddRange(Astar.FindPath(astar.nodeGrid, transform.position, astar.target.position));

        if (!astar.paths.Contains(path))
            astar.paths.Add(path);

        if (path != null && path.Count > 0)
        {
            closestNode = path[0];
            transform.position = Vector3.Lerp(transform.position, path[1].position, speed * Time.deltaTime);
        }
    }
}
