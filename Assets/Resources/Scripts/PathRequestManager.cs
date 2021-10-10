using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    static PathRequestManager Instance;
    public Astar astar;

    Queue<PathRequest> requests = new Queue<PathRequest>();
    bool isProcessing = false;
    Queue<PathResult> results = new Queue<PathResult>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            for (int i = 0; i < itemsInQueue; i++)
            {
                PathResult result = results.Dequeue();
                result.callback(result.path, result.success);
            }
        }

        if (!isProcessing && requests.Count > 0)
        {
            isProcessing = true;
            ParseRequest(Instance.requests.Dequeue());
        }
    }

    public static void RequestPath(PathRequest request)
    {
        Instance.requests.Enqueue(request);
    }

    void ParseRequest(PathRequest request)
    {
        StartCoroutine(Instance.astar.FindPath(request, Instance.FinishedProcessingPath));
    }

    public void FinishedProcessingPath(PathResult result)
    {
        results.Enqueue(result);

        isProcessing = false;
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

public struct PathRequest
{
    public AstarGrid grid;
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(AstarGrid _grid, Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
    {
        grid = _grid;
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}
