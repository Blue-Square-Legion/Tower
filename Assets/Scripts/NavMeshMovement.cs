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
    private NavMeshPath path;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
        path = agent.path;
    }

    void Update()
    {
        
        //NavMeshPathStatus pathStatus = agent.pathStatus;
        agent.CalculatePath(target.position, path);
        
        print(path.status);
    }
}
