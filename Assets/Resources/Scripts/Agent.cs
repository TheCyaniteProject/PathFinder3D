using UnityEngine;
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    public AstarGrid astar;
    public new Rigidbody rigidbody;
    public Transform rotationHost;
    public float maxGridDistance = 3;
    [Header("Follow Settings")]
    public Transform target;
    public float pathUpdateTime = 0.5f;
    public bool targetPlayer = false;
    public float maxIncline = 2;
    public float stopDistance = 4;
    public float runStartDistance = 10;
    public float runStopDistance = 4;
    public bool allowRunning = true;
    public float walkSpeed = 15;
    public float runSpeed = 30;
    public float rotationSpeed = 1;
    [Header("Animations")]
    public Animator animator;
    public float animationSpeed = 1;
    public States state = States.None;
    States _state = States.None;
    public string idleAnimationName = "Idle";
    public string walkAnimationName = "Walk";
    public string runAnimationName = "Run";
    public string attackAnimationName = "Attack";

    // Private
    public bool isRunning = false;
    public Vector3[] path;
    float speed = 0;
    Node closestNode;

    public enum States
    {
        Idle,
        Walk,
        Run,
        Attack,
        None
    }

    private void Start()
    {
        if (!astar)
            astar = AstarGrid.Instance;
        if (targetPlayer)
            target = astar.target;
    }

    public void OnPathFound(Vector3[] newPath, bool success)
    {
        if (success)
        {
            path = newPath;
        }
        else
        {
            path = null;
            timer += 2.5f;
        }
    }

    //*
    float timer = 0;
    public float stuckTimer = 0;
    Vector3 stuckPosition = Vector3.zero;
    Vector3 direction;
    Vector3 lookDirection;
    private void Update()
    {
        if (!target) return;

        speed = 0;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = pathUpdateTime;
            if (AstarGrid.Instance.nodeGrid != null)
                PathRequestManager.RequestPath(new PathRequest(AstarGrid.Instance.nodeGrid, transform.position, target.position, OnPathFound));
        }

        if (Vector3.Distance(transform.position, astar.transform.position) < astar.gridSize + astar.gridSize / 2) // if we're not to far from the grid
        {
            if (closestNode == null || Vector3.Distance(closestNode.position, transform.position) > maxGridDistance) // and we're too far from the closest node
            {
                closestNode = AstarGrid.Instance.NodeFromWorldPoint(transform.position);

                if (closestNode == null)
                {
                    path = null;
                    return;
                }
            }
        }
        else
        {
            // disable physics
        }

        if (path != null && path.Length > 0)
        {

            if (isRunning || Vector3.Distance(transform.position, target.position) >= runStartDistance && allowRunning)
            {
                isRunning = true;
                speed = runSpeed;
            }
            
            if (!isRunning || Vector3.Distance(transform.position, target.position) <= runStopDistance || !allowRunning)
            {
                isRunning = false;
                speed = walkSpeed;
            }

            // Movement
            if (Vector3.Distance(transform.position, target.position) >= stopDistance)
            {
                direction = path[0] - transform.position;
                if (path.Length > 4)
                    lookDirection = path[0] - transform.position;
                else if (target)
                    lookDirection = target.position - transform.position;
            }
            else
                speed = 0;
        }

        if (speed > 0 && Vector3.Distance(transform.position, stuckPosition) < 0.1f && path.Length > 0)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0;
            stuckPosition = transform.position;
        }

        if (stuckTimer > 20 && path.Length > 0)
        {
            transform.position = Vector3.Lerp(path[0], transform.position, 0.5f);
        }

        if (!animator) return;

        // Animation
        state = States.Idle;
        if (speed > 0)
            state = States.Walk;
        if (speed > walkSpeed && allowRunning)
            state = States.Run;

        animator.SetFloat("AnimationSpeed", animationSpeed);

        if (_state != state)
        {
            animator.IsInTransition(0);

            switch (state)
            {
                case States.Idle:
                    animator.CrossFade("Base Layer." + idleAnimationName, 0.2f);
                    break;
                case States.Walk:
                    animator.CrossFade("Base Layer." + walkAnimationName, 0.2f);
                    break;
                case States.Run:
                    animator.CrossFade("Base Layer." + runAnimationName, 0.3f);
                    break;
                case States.Attack:
                    animator.CrossFade("Base Layer." + attackAnimationName, 0.1f);
                    break;
            }

            _state = state;
        }
        
    }//*/

    private void FixedUpdate()
    {
        if (!target || path == null || path.Length == 0)
            return;

        // Movement

        rigidbody.AddForce(direction.normalized * speed * 100 * Time.deltaTime, ForceMode.Force);

        Vector3 newVelocity = Vector3.ClampMagnitude(rigidbody.velocity, speed * 10 * Time.deltaTime);

        rigidbody.velocity = new Vector3(newVelocity.x, rigidbody.velocity.y, newVelocity.z);

        // Rotation

        float singleStep = rotationSpeed * Time.deltaTime;

        Vector3 newDirection = Vector3.RotateTowards(rotationHost.forward, lookDirection, singleStep, 0.0f);

        rotationHost.rotation = Quaternion.LookRotation(newDirection);
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i < 2)
                    Gizmos.DrawLine(transform.position, path[1]);
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
