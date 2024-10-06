using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI;
using UnityEngine.AI;
using System.IO;

public class NavMeshMovement : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;

    void Start()
    {
        target = GameObject.Find("EndPoint").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
    }

    public bool ReachedEnd()
    {
        if (agent != null && agent.remainingDistance <= 1)
        {
            return true;
        }
        //if (agent.remainingDistance <= 1)
        //    return true;
        else return false;

    }
}
