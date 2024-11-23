using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class DrawPath : MonoBehaviour
{
    private LineRenderer line;
    private Transform target; 
    private NavMeshAgent agent;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        agent = GetComponent<NavMeshAgent>();
        
        line.startWidth = 0.15f;
        line.endWidth = 0.15f;
        line.positionCount = 0;
        DrawAgentPath();
    }

    private void Update()
    {
        if (agent.path.status == NavMeshPathStatus.PathComplete)
            DrawAgentPath();
    }

    public void DrawAgentPath()
    {
        line.positionCount = agent.path.corners.Length;

        if (agent.path.corners.Length < 2)
            return;

        line.SetPosition(0, transform.position);

        for (var i = 1; i < agent.path.corners.Length; i++)
        {
            line.SetPosition(i, agent.path.corners[i]);
        }
    }
}
