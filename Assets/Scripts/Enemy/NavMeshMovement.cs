using UnityEngine;
using UnityEngine.AI;
using System;

public class NavMeshMovement : MonoBehaviour
{
    [NonSerialized] public Transform target;
    [SerializeField] public NavMeshAgent agent;
    private Vector3 originalPositon;
    public float remainingDist;

    void Start()
    {
        target = GameObject.Find("EndPoint").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
        originalPositon = gameObject.transform.position;
    }

    private void Update()
    {
        remainingDist = agent.remainingDistance;
    }

    public bool ReachedEnd()
    {
        if (agent.hasPath && agent.remainingDistance <= 1)
        {
            print(agent.remainingDistance);
            return true;
        }
        else return false;
    }

    public void SetSpeed(float speed)
    {
        if (agent != null)
            agent.speed = speed;
    }

    public float GetSpeed()
    {
        return agent.speed;
    }

    public void ResetDestination()
    {
        if(target != null)
            agent.SetDestination(target.position);
        originalPositon = gameObject.transform.position;
    }

    public void FlipDirection(int option)
    {
        if (option == 0)
        {
            agent.SetDestination(originalPositon);
        } else
        {
            agent.SetDestination(target.position);
        }
    }
}