using UnityEngine;
using UnityEngine.AI;
using System;

public class NavMeshMovement : MonoBehaviour
{
    [NonSerialized] public Transform target;
    [SerializeField] private NavMeshAgent agent;

    void Start()
    {
        target = GameObject.Find("EndPoint").transform;
        //agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
    }

    public bool ReachedEnd()
    {
        if (agent != null && agent.remainingDistance <= 1)
        {
            return true;
        }
        else return false;
    }

    public void SetSpeed(float speed)
    {
        print(agent);
        agent.speed = speed;
    }
}